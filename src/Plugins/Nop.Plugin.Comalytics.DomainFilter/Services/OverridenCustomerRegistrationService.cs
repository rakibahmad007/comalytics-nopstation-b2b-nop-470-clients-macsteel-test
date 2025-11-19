using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Comalytics.DomainFilter.Services
{
    public class OverridenCustomerRegistrationService : CustomerRegistrationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly DomainFilterSettings _domainFilterSettings;
        private readonly ICustomerService _customerService;
        private readonly IDomainFilterService _domainFilterService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IRewardPointService _rewardPointService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public static string B2BCustomerRoleSystemName => "B2BCustomer";

        public OverridenCustomerRegistrationService(
            CustomerSettings customerSettings,
            IActionContextAccessor actionContextAccessor,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IRewardPointService rewardPointService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            RewardPointsSettings rewardPointsSettings,
            IHttpContextAccessor httpContextAccessor,
            IDomainFilterService domainFilterService,
            DomainFilterSettings domainFilterSettings
        )
            : base(
                customerSettings,
                actionContextAccessor,
                authenticationService,
                customerActivityService,
                customerService,
                encryptionService,
                eventPublisher,
                genericAttributeService,
                localizationService,
                multiFactorAuthenticationPluginManager,
                newsLetterSubscriptionService,
                notificationService,
                permissionService,
                rewardPointService,
                shoppingCartService,
                storeContext,
                storeService,
                urlHelperFactory,
                workContext,
                workflowMessageService,
                rewardPointsSettings
            )
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _localizationService = localizationService;
            _rewardPointService = rewardPointService;
            _rewardPointsSettings = rewardPointsSettings;
            _httpContextAccessor = httpContextAccessor;
            _domainFilterService = domainFilterService;
            _domainFilterSettings = domainFilterSettings;
        }

        #endregion

        #region Ctor



        #endregion

        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public override async Task<CustomerRegistrationResult> RegisterCustomerAsync(
            CustomerRegistrationRequest request
        )
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Customer == null)
                throw new ArgumentException("Can't load current customer");

            var result = new CustomerRegistrationResult();

            if (request.Customer.IsSearchEngineAccount())
            {
                result.AddError("Search engine can't be registered");
                return result;
            }

            if (request.Customer.IsBackgroundTaskAccount())
            {
                result.AddError("Background task account can't be registered");
                return result;
            }

            if (await _customerService.IsRegisteredAsync(request.Customer, true))
            {
                result.AddError("Current customer is already registered");
                return result;
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                result.AddError(
                    await _localizationService.GetResourceAsync(
                        "Account.Register.Errors.EmailIsNotProvided"
                    )
                );
                return result;
            }

            if (!CommonHelper.IsValidEmail(request.Email))
            {
                result.AddError(await _localizationService.GetResourceAsync("Common.WrongEmail"));
                return result;
            }

            #region DomainFilter
            var path = _httpContextAccessor.HttpContext.Request.Path;
            var isB2CRegistration =
                !string.IsNullOrWhiteSpace(path) && path.Value.ToLower().Contains("b2cregister");

            // If domain filtering is enabled and email or domain is blacklisted
            if (
                _domainFilterSettings.EnableFilter
                && isB2CRegistration
                && _domainFilterService.IsDomainOrEmailBlacklisted(request.Email)
            )
            {
                result.AddError(
                    await _localizationService.GetResourceAsync(
                        "Plugins.Comalytics.DomainFilter.Domain.Blacklisted"
                    )
                );
                return result;
            }
            #endregion

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(
                    await _localizationService.GetResourceAsync(
                        "Account.Register.Errors.PasswordIsNotProvided"
                    )
                );
                return result;
            }

            if (_customerSettings.UsernamesEnabled && string.IsNullOrEmpty(request.Username))
            {
                result.AddError(
                    await _localizationService.GetResourceAsync(
                        "Account.Register.Errors.UsernameIsNotProvided"
                    )
                );
                return result;
            }

            // Validate unique user
            if (await _customerService.GetCustomerByEmailAsync(request.Email) != null)
            {
                result.AddError(
                    await _localizationService.GetResourceAsync(
                        "Account.Register.Errors.EmailAlreadyExists"
                    )
                );
                return result;
            }

            if (
                _customerSettings.UsernamesEnabled
                && await _customerService.GetCustomerByUsernameAsync(request.Username) != null
            )
            {
                result.AddError(
                    await _localizationService.GetResourceAsync(
                        "Account.Register.Errors.UsernameAlreadyExists"
                    )
                );
                return result;
            }

            // At this point, request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;

            var customerPassword = new CustomerPassword
            {
                CustomerId = request.Customer.Id,
                PasswordFormat = request.PasswordFormat,
                CreatedOnUtc = DateTime.UtcNow,
            };

            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    customerPassword.Password = request.Password;
                    break;
                case PasswordFormat.Encrypted:
                    customerPassword.Password = _encryptionService.EncryptText(request.Password);
                    break;
                case PasswordFormat.Hashed:
                    var saltKey = _encryptionService.CreateSaltKey(
                        NopCustomerServicesDefaults.PasswordSaltKeySize
                    );
                    customerPassword.PasswordSalt = saltKey;
                    customerPassword.Password = _encryptionService.CreatePasswordHash(
                        request.Password,
                        saltKey,
                        _customerSettings.HashedPasswordFormat
                    );
                    break;
            }

            await _customerService.InsertCustomerPasswordAsync(customerPassword);

            request.Customer.Active = request.IsApproved;

            // Add to 'Registered' role
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(
                NopCustomerDefaults.RegisteredRoleName
            );
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");

            await _customerService.AddCustomerRoleMappingAsync(
                new CustomerCustomerRoleMapping
                {
                    CustomerId = request.Customer.Id,
                    CustomerRoleId = registeredRole.Id,
                }
            );
            // Fetch the guest role using the new GetCustomerRoleBySystemNameAsync method
            var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(
                NopCustomerDefaults.GuestsRoleName
            );

            if (await _customerService.IsGuestAsync(request.Customer))
            {
                await _customerService.RemoveCustomerRoleMappingAsync(request.Customer, guestRole);
            }

            // Add reward points for customer registration (if enabled)
            if (_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForRegistration > 0)
            {
                var endDate =
                    _rewardPointsSettings.RegistrationPointsValidity > 0
                        ? (DateTime?)
                            DateTime.UtcNow.AddDays(
                                _rewardPointsSettings.RegistrationPointsValidity.Value
                            )
                        : null;
                await _rewardPointService.AddRewardPointsHistoryEntryAsync(
                    request.Customer,
                    _rewardPointsSettings.PointsForRegistration,
                    request.StoreId,
                    await _localizationService.GetResourceAsync(
                        "RewardPoints.Message.EarnedForRegistration"
                    ),
                    endDate: endDate
                );
            }

            await _customerService.UpdateCustomerAsync(request.Customer);

            return result;
        }

        /// <summary>
        /// Validate customer
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        public override async Task<CustomerLoginResults> ValidateCustomerAsync(
            string usernameOrEmail,
            string password
        )
        {
            var customer = _customerSettings.UsernamesEnabled
                ? await _customerService.GetCustomerByUsernameAsync(usernameOrEmail)
                : await _customerService.GetCustomerByEmailAsync(usernameOrEmail);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;

            #region DomainFilter
            // B2B Customers will not be affected by domain filter (2417)
            if (!await _customerService.IsInCustomerRoleAsync(customer, B2BCustomerRoleSystemName))///
            {
                // if domain filtering is enabled and email or domain is blacklisted
                if (
                    _domainFilterSettings.EnableFilter
                    && _domainFilterService.IsDomainOrEmailBlacklisted(customer.Email)
                )
                    return CustomerLoginResults.LockedOut;
            }
            #endregion

            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;

            // only registered can log in
            if (!await _customerService.IsRegisteredAsync(customer))
                return CustomerLoginResults.NotRegistered;

            // check whether a customer is locked out
            if (
                customer.CannotLoginUntilDateUtc.HasValue
                && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow
            )
                return CustomerLoginResults.LockedOut;

            if (
                !PasswordsMatch(
                    await _customerService.GetCurrentPasswordAsync(customer.Id),
                    password
                )
            )
            {
                // wrong password
                customer.FailedLoginAttempts++;
                if (
                    _customerSettings.FailedPasswordAllowedAttempts > 0
                    && customer.FailedLoginAttempts
                        >= _customerSettings.FailedPasswordAllowedAttempts
                )
                {
                    // lock out
                    customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(
                        _customerSettings.FailedPasswordLockoutMinutes
                    );
                    // reset the counter
                    customer.FailedLoginAttempts = 0;
                }

                await _customerService.UpdateCustomerAsync(customer);

                return CustomerLoginResults.WrongPassword;
            }

            // update login details
            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            await _customerService.UpdateCustomerAsync(customer);

            return CustomerLoginResults.Successful;
        }
    }
}
