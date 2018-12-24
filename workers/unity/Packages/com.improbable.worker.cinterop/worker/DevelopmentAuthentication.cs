using Improbable.Worker.CInterop.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Improbable.Worker.CInterop.Alpha
{
    public struct PlayerIdentityTokenResponse
    {
        public string PlayerIdentityToken;
        public ConnectionStatusCode Status;
        public string Error;
    }

    public struct LoginTokenDetails
    {
        public string DeploymentId;
        public string DeploymentName;
        public List<string> Tags;
        public string LoginToken;
    }

    public struct LoginTokensResponse
    {
        public List<LoginTokenDetails> LoginTokens;
        public ConnectionStatusCode Status;
        public string Error;
    }

    public class DevelopmentAuthentication
    {
        public static unsafe Future<PlayerIdentityTokenResponse?>
            CreateDevelopmentPlayerIdentityTokenAsync(string hostname, ushort port, PlayerIdentityTokenRequest request)
        {
            Alpha_PlayerIdentityTokenResponseFutureHandle future = null;
            ParameterConversion.ConvertPlayerIdentityTokenRequest(request, parameters =>
            {
                fixed(byte* hostnameBytes = ApiInterop.ToUtf8Cstr(hostname))
                {
                    future = CWorker.Alpha_CreateDevelopmentPlayerIdentityTokenAsync(hostnameBytes, port, parameters);
                }
            });
            return new Future<PlayerIdentityTokenResponse?>(future,
                        ParameterConversion.PlayerIdentityTokenResponseFutureGet(future));
        }

        public static unsafe Future<Alpha.LoginTokensResponse?>
            CreateDevelopmentLoginTokensAsync(string hostname, ushort port, LoginTokensRequest request)
        {
            Alpha_LoginTokensResponseFutureHandle future = null;
            ParameterConversion.ConvertLoginTokensRequest(request, parameters =>
            {
                fixed(byte* hostnameBytes = ApiInterop.ToUtf8Cstr(hostname))
                {
                    future = CWorker.Alpha_CreateDevelopmentLoginTokensAsync(hostnameBytes, port, parameters);
                }
            });
            return new Future<Alpha.LoginTokensResponse?>(future,
                        ParameterConversion.LoginTokenDetailsFutureGet(future));
        }
    }
}