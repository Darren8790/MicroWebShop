namespace IdentityService.Models.ManageViewModels;

    public record IndexViewModel
    {
        public bool HasPassword { get; init; }

        public IList<UserLoginInfo> Logins { get; init; }

        public bool BrowserRemembered { get; init; }
    }