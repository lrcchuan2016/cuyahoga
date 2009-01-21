﻿using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Castle.Core.Logging;
using Cuyahoga.Core;
using Cuyahoga.Core.Validation;
using Cuyahoga.Web.Manager.Filters;
using Cuyahoga.Web.Manager.Model.ViewModels;
using Cuyahoga.Web.Mvc;
using Resources.Cuyahoga.Web.Manager;

namespace Cuyahoga.Web.Manager.Controllers
{
	/// <summary>
	/// Base class for all controllers.
	/// </summary>
	[SiteFilter(Order = 1)]
	[MenuDataFilter(Order = 2)]
	[ExceptionFilter(ExceptionType = typeof(SecurityException))]
	public abstract class BaseController : Controller
	{
		private ICuyahogaContext _cuyahogaContext;
		private ILogger _logger = NullLogger.Instance;
		private MessageViewData _messageViewData;
		private IModelValidator _modelValidator;

		/// <summary>
		/// Get or sets the Cuyahoga context.
		/// </summary>
		public ICuyahogaContext CuyahogaContext
		{
			get { return this._cuyahogaContext; }
			set { this._cuyahogaContext = value; }
		}

		/// <summary>
		/// Gets or sets the logger.
		/// </summary>
		public ILogger Logger
		{
			get { return this._logger; }
			set { this._logger = value; }
		}

		/// <summary>
		/// Sets the model validator.
		/// </summary>
		/// <remarks>
		/// We could include this dependency as a constructor parameter, but this forces inheritors to also include this parameter.
		/// This way, inheriting controllers need to explicitly set the validator.
		/// </remarks>
		protected IModelValidator ModelValidator
		{
			set { this._modelValidator = value; }
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			InitMessageViewData();
			base.OnActionExecuting(filterContext);
		}

		protected override void  OnActionExecuted(ActionExecutedContext filterContext)
		{
			DisplayModelStateErrors();
			base.OnActionExecuted(filterContext);
		}

		/// <summary>
		/// Validates the given object. If invalid, the errors are added to the ModelState.
		/// </summary>
		/// <param name="objectToValidate">The object to validate</param>
		/// <returns>True if the object is valid</returns>
		protected virtual bool ValidateModel(object objectToValidate)
		{
			return ValidateModel(objectToValidate, null, null, null);
		}

		/// <summary>
		/// Validates the given object. If invalid, the errors are added to the ModelState.
		/// </summary>
		/// <param name="objectToValidate">The object to validate</param>
		/// <param name="includeProperties">Properties to check</param>
		/// <returns>True if the object is valid</returns>
		protected virtual bool ValidateModel(object objectToValidate, string[] includeProperties)
		{
			return ValidateModel(objectToValidate, null, includeProperties);
		}

		/// <summary>
		/// Validates the given object. If invalid, the errors are added to the ModelState.
		/// </summary>
		/// <param name="objectToValidate">The object to validate</param>
		/// <param name="includeProperties">Properties to check</param>
		/// <param name="modelPrefix">The prefix of the form elements</param>
		/// <returns>True if the object is valid</returns>
		protected virtual bool ValidateModel(object objectToValidate, string[] includeProperties, string modelPrefix)
		{
			return ValidateModel(objectToValidate, null, includeProperties, modelPrefix);
		}

		/// <summary>
		/// Validates the given object. If invalid, the errors are added to the ModelState.
		/// </summary>
		/// <param name="objectToValidate">The object to validate</param>
		/// <param name="modelValidator">A specific model validator</param>
		/// <param name="includeProperties">Properties to check</param>
		/// <returns>True if the object is valid</returns>
		protected virtual bool ValidateModel(object objectToValidate, IModelValidator modelValidator, string[] includeProperties)
		{
			return ValidateModel(objectToValidate, modelValidator, includeProperties, null);
		}

		/// <summary>
		/// Validates the given object. If invalid, the errors are added to the ModelState.
		/// </summary>
		/// <param name="objectToValidate">The object to validate</param>
		/// <param name="modelValidator">A specific model validator</param>
		/// <param name="includeProperties">Properties to check</param>
		/// <param name="modelPrefix"></param>
		/// <returns>True if the object is valid</returns>
		protected virtual bool ValidateModel(object objectToValidate, IModelValidator modelValidator, string[] includeProperties, string modelPrefix)
		{
			if (modelValidator == null && this._modelValidator == null)
			{
				throw new InvalidOperationException("A call to Validate() was made while there is no IModelValidator available to perform validation.");
			}
			// if a specific modelvalidator is passed, use that one, otherwise use the modelvalidator of the controller.
			IModelValidator modelValidatorToUse = modelValidator ?? this._modelValidator;

			if (!modelValidatorToUse.IsValid(objectToValidate, includeProperties))
			{
				IDictionary<string, ICollection<string>> errorsForProperties = modelValidatorToUse.GetErrors();
				foreach (KeyValuePair<string, ICollection<string>> errorsForProperty in errorsForProperties)
				{
					string propertyName = errorsForProperty.Key;
					if (! String.IsNullOrEmpty(modelPrefix))
					{
						propertyName = modelPrefix + "." + propertyName;
					}
					foreach (string errorMessage in errorsForProperty.Value)
					{
						ViewData.ModelState.AddModelError(propertyName, errorMessage);
					}
				}
				return false;
			}
			return true;
		}

		private void DisplayModelStateErrors()
		{
			// Show the ModelState errors in the standard Cuyahoga errorbox.
			if (! ViewData.ModelState.IsValid)
			{
				string generalMessage = GlobalResources.ModelValidationErrorMessage;
				TagBuilder errorList = new TagBuilder("ul");
				StringBuilder errorSummary = new StringBuilder();
				foreach (KeyValuePair<string, ModelState> modelStateKvp in ViewData.ModelState)
				{
					foreach (ModelError modelError in modelStateKvp.Value.Errors)
					{
						TagBuilder listItem = new TagBuilder("li");
						listItem.SetInnerText(modelError.ErrorMessage);
						errorSummary.AppendLine(listItem.ToString(TagRenderMode.Normal));
					}
				}
				errorList.InnerHtml = errorSummary.ToString();
				RegisterMessage(MessageType.Error, generalMessage + errorList.ToString(TagRenderMode.Normal), false);
			}
		}

		protected virtual void ShowMessage(string message)
		{
			ShowMessage(message, false);
		}

		protected virtual void ShowMessage(string message, bool persistForNextRequest)
		{
			RegisterMessage(MessageType.Message, message, persistForNextRequest);
		}

		protected virtual void ShowError(string error)
		{
			ShowError(error, false);
		}

		protected virtual void ShowError(string error, bool persistForNextRequest)
		{
			RegisterMessage(MessageType.Error, error, persistForNextRequest);
		}

		protected virtual void ShowException(Exception exception)
		{
			ShowException(exception, false);
		}

		protected virtual void ShowException(Exception exception, bool persistForNextRequest)
		{
			RegisterMessage(MessageType.Exception, exception.Message, persistForNextRequest);
			while (exception.InnerException != null)
			{
				exception = exception.InnerException;
				RegisterMessage(MessageType.Exception, exception.Message, persistForNextRequest);
			}
		}

		/// <summary>
		/// Gets a translation of the given message from the GlobaResources. If not found, the original message is returned.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		protected string TranslateMessage(string message)
		{
			return GlobalResources.ResourceManager.GetString(message, Thread.CurrentThread.CurrentUICulture) ?? message;
		}

		private void InitMessageViewData()
		{
			if (TempData.ContainsKey("Messages"))
			{
				this._messageViewData = new MessageViewData((MessageViewData)TempData["Messages"]);
			}
			else
			{
				this._messageViewData = new MessageViewData();
			}
			ViewData["Messages"] = this._messageViewData;
		}

		private void RegisterMessage(string messageType, string message, bool persistForNextRequest)
		{
			string localizedMessage = TranslateMessage(message);
			this._messageViewData.AddMessage(messageType, localizedMessage, persistForNextRequest);
			if (persistForNextRequest)
			{
				// Just persist all messages directly. Not very subtle, but this also works in situations where exceptions occur.
				PersistMessages();
			}
		}

		private void PersistMessages()
		{
			if (this._messageViewData.HasFlashMessages)
			{
				TempData["Messages"] = this._messageViewData.GetFlashMessages();
			}
		}
	}
}