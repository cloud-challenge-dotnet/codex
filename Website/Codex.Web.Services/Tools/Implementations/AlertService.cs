using Codex.Web.Services.Models;
using Codex.Web.Services.Tools.Interfaces;
using System;

namespace Codex.Web.Services.Tools.Implementations
{
    public class AlertService : IAlertService
    {
        private const string _defaultId = "default-alert";
        public event Action<Alert>? OnAlert;

        public void Success(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new Alert
            {
                Id = id,
                Type = AlertType.Success,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose
            });
        }

        public void Error(string message, string? id= null, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new Alert
            {
                Id = id,
                Type = AlertType.Error,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose
            });
        }

        public void Info(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new Alert
            {
                Id = id,
                Type = AlertType.Info,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose
            });
        }

        public void Warn(string message, string? id = null, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new Alert
            {
                Id = id,
                Type = AlertType.Warning,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose
            });
        }

        public void Alert(Alert alert)
        {
            alert.Id ??= _defaultId;
            this.OnAlert?.Invoke(alert);
        }

        public void Clear(string? id = null)
        {
            id ??= _defaultId;
            this.OnAlert?.Invoke(new Alert { Id = id });
        }
    }
}
