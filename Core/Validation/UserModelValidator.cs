﻿using System;
using System.Collections.Generic;
using Cuyahoga.Core.Domain;
using Cuyahoga.Core.Service.Membership;

namespace Cuyahoga.Core.Validation
{
	public class UserModelValidator : CastleModelValidator<User>
	{
		private readonly IUserService _userService;

		public UserModelValidator(IUserService userService)
		{
			_userService = userService;
		}

		protected override void PerformValidation(User objectToValidate, ICollection<string> includeProperties)
		{
			base.PerformValidation(objectToValidate, includeProperties);

			// Check username uniqueness.
			if (ShouldValidateProperty("UserName", includeProperties)
				&& ! String.IsNullOrEmpty(objectToValidate.UserName))
			{
				if (this._userService.FindUsersByUsername(objectToValidate.UserName).Count > 0)
				{
					AddError("UserName", "UserNameValidatorNotUnique", true);
				}
			}
		}
	}
}
