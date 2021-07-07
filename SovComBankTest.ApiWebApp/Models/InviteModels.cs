﻿using System.ComponentModel.DataAnnotations;
using SovComBankTest.Services.Abstractions;

namespace SovComBankTest.ApiWebApp.Models
{
    /// <summary>
    ///     Запрос с номерами телефонов и сообщением указанным адресатам
    /// </summary>
    public class InviteMessage
    {
        /// <summary>
        ///     Список уникальных номеров
        /// </summary>
        /// <example>["79998887766", "75554443322"]</example>
        public string[]? Phones { get; init; }

        /// <summary>
        ///     Сообщение в формате GSM для отправки указанным адресатам
        /// </summary>
        /// <example>Hello</example>
        public string? Message { get; init; }

        /// <summary>
        ///     Идентификатор АПИ
        /// </summary>
        /// <example>4</example>
        public int? ApiId { get; set; }
    }


    internal class InviteMessageValidationAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not InviteMessage inviteMessage)
                return false;

            var phones = inviteMessage.Phones;

            if (!ISmsService.TryValidatePhones(phones, out var error))
            {
                ErrorMessage = error;
                return false;
            }

            if (!ISmsService.TryValidateMessage(inviteMessage.Message, out error))
            {
                ErrorMessage = error;
                return false;
            }

            return true;
        }
    }
}