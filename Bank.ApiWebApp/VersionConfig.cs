using Microsoft.AspNetCore.Mvc;

namespace Bank.ApiWebApp
{
    /// <summary>
    ///     ������������ ������������ API
    /// </summary>
    internal sealed class VersionConfig
    {
        /// <summary>
        ///     �������� � ������� ��� ���������, ������������� �� ���������� ������ �������������� API
        /// </summary>
        public const string ApiVersionParameterName = "api-version";

        /// <summary>
        ///     ������ API �� ���������
        /// </summary>
        public static readonly ApiVersion DefaultApiVersion = new(1, 0);

        /// <summary>
        ///     �������� ������ ���
        /// </summary>
        public static readonly string[] ExistingVersions = {"1.0"};
    }
}