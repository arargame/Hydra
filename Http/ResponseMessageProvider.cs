﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public static class ResponseMessageProvider
    {
        public static List<ResponseObjectMessage> GetDefaultMessages(string? actionName)
        {
            return new List<ResponseObjectMessage>
        {
            new ResponseObjectMessage(title: actionName, text: "The process has been completed successfully", showWhenSuccess: true),
            new ResponseObjectMessage(title: actionName, text: "Something went wrong. Please contact IT team at 'info@hydra.com'", showWhenSuccess: false)
        };
        }

        public static ResponseObjectMessage GetCustomErrorMessage(string title, string text)
        {
            return new ResponseObjectMessage(title, text, showWhenSuccess: false);
        }

        public static ResponseObjectMessage FromValidationResult(ValidationResult validation)
        {
            return new ResponseObjectMessage(
                title: string.Join(",", validation.MemberNames),
                text: validation.ErrorMessage ?? "",
                showWhenSuccess: false);
        }
    }

}
