using Codex.Web.Services.Models;
using System;

namespace Codex.Web.Services.Tools.Interfaces
{
    public interface IAlertService
    {
        event Action<Alert> OnAlert;
        void Success(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true);
        void Error(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true);
        void Info(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true);
        void Warn(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true);
        void Alert(Alert alert);
        void Clear(string? id = null);
    }
}
