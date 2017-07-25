using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Wodsoft.Net.Http
{
    public class ApiChannelBuilder
    {
        private static AssemblyBuilder _AssemblyBuilder;
        private static ModuleBuilder _ModuleBuilder;
        static ApiChannelBuilder()
        {
            _AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Wodsoft.Net.Http.Dynamic"), AssemblyBuilderAccess.Run);
            _ModuleBuilder = _AssemblyBuilder.DefineDynamicModule("ApiChannel");
        }

        private HttpClient _Client;
        private ConcurrentDictionary<Type, object> _Channels;
        private IApiFormatter _Formatter;

        public ApiChannelBuilder(HttpClient client, IApiFormatter formatter)
        {
            _Client = client ?? throw new ArgumentNullException(nameof(client));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _Channels = new ConcurrentDictionary<Type, object>();
        }

        public T GetChannel<T>()
        {
            return (T)_Channels.GetOrAdd(typeof(T), type =>
            {
                var apiChannelAttribute = type.GetTypeInfo().GetCustomAttribute<ApiChannelAttribute>();
                if (apiChannelAttribute == null)
                    throw new NotSupportedException("该接口未定义ApiChannel特性。");
                var moduleBuilder = _ModuleBuilder;
                var typeBuilder = moduleBuilder.DefineType(type.Namespace + ".Proxy_" + type.Name,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                    typeof(ApiChannel), new Type[] { type });
                BuildConstructor(typeBuilder, type, apiChannelAttribute.ChannelName);
                foreach (var method in type.GetTypeInfo().DeclaredMethods)
                {
                    BuildMethod(typeBuilder, method);
                }
                var typeInfo = typeBuilder.CreateTypeInfo();
                var channel = Activator.CreateInstance(typeInfo.AsType(), _Client, _Formatter);
                return channel;
            });
        }

        private static readonly ConstructorInfo _ApiChannelConstructor = typeof(ApiChannel).GetTypeInfo().DeclaredConstructors.First();
        protected void BuildConstructor(TypeBuilder typeBuilder, Type channelType, string channelName)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, CallingConventions.Standard | CallingConventions.HasThis, new Type[] { typeof(HttpClient), typeof(IApiFormatter) });
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldstr, channelName);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Call, _ApiChannelConstructor);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static readonly ConstructorInfo _DictionaryConstructor = typeof(Dictionary<string, object>).GetTypeInfo().DeclaredConstructors.Where(t => t.GetParameters().Length == 0).First();
        private static readonly MethodInfo _DictionaryAdd = typeof(Dictionary<string, object>).GetRuntimeMethod("Add", new Type[] { typeof(string), typeof(object) });
        private static readonly MethodInfo _ApiChannelExecuteAsync = typeof(ApiChannel).GetTypeInfo().GetDeclaredMethod("ExecuteAsync");
        protected void BuildMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            if (!method.ReturnType.GetTypeInfo().IsGenericType || method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
                throw new NotSupportedException("不支持Task<>以外的返回类型。");
            var methodBuilder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                method.ReturnType, method.GetParameters().Select(t => t.ParameterType).ToArray());
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterBuilder = methodBuilder.DefineParameter(i, parameters[i].Attributes, parameters[i].Name);
                if (parameters[i].HasDefaultValue)
                    parameterBuilder.SetConstant(parameters[i].DefaultValue);
            }
            var ilGenerator = methodBuilder.GetILGenerator();
            var values = ilGenerator.DeclareLocal(typeof(Dictionary<string, object>));
            ilGenerator.Emit(OpCodes.Newobj, _DictionaryConstructor);
            ilGenerator.Emit(OpCodes.Stloc, values);
            for (int i = 0; i < parameters.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldloc, values);
                ilGenerator.Emit(OpCodes.Ldstr, parameters[i].Name);
                ilGenerator.Emit(OpCodes.Ldarg_S, i + 1);
                if (parameters[i].ParameterType.GetTypeInfo().IsValueType)
                    ilGenerator.Emit(OpCodes.Box, parameters[i].ParameterType);
                ilGenerator.Emit(OpCodes.Call, _DictionaryAdd);
            }
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldstr, method.Name);
            ilGenerator.Emit(OpCodes.Ldloc, values);
            ilGenerator.Emit(OpCodes.Call, _ApiChannelExecuteAsync.MakeGenericMethod(method.ReturnType.GenericTypeArguments));
            ilGenerator.Emit(OpCodes.Ret);
            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }
    }
}
