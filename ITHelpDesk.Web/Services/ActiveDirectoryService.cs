using ITHelpDesk.Web.Models;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ITHelpDesk.Web.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly IConfiguration _configuration;
        private readonly string _domain;
        private readonly string _ldapPath;
        private readonly string _itSupportGroup;

        public ActiveDirectoryService(IConfiguration configuration)
        {
            _configuration = configuration;
            _domain = _configuration["ActiveDirectory:Domain"] ?? throw new InvalidOperationException("AD Domain not configured");
            _ldapPath = _configuration["ActiveDirectory:LdapPath"] ?? throw new InvalidOperationException("LDAP Path not configured");
            _itSupportGroup = _configuration["ActiveDirectory:ITSupportGroup"] ?? "IT Support";
        }

        public ADUser? GetUserByUsername(string username)
        {
            try
            {
                // Remove domain prefix if present (DOMAIN\username -> username)
                if (username.Contains("\\"))
                {
                    username = username.Split('\\')[1];
                }

                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
                {
                    if (userPrincipal == null)
                        return null;

                    return MapUserPrincipalToADUser(userPrincipal);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting user by username: {ex.Message}");
                return null;
            }
        }

        public ADUser? GetUserByEmail(string email)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, email))
                {
                    if (userPrincipal == null)
                        return null;

                    return MapUserPrincipalToADUser(userPrincipal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email: {ex.Message}");
                return null;
            }
        }

        public List<ADUser> SearchUsers(string searchTerm)
        {
            var users = new List<ADUser>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var userPrincipal = new UserPrincipal(context))
                {
                    userPrincipal.SamAccountName = $"*{searchTerm}*";
                    using (var searcher = new PrincipalSearcher(userPrincipal))
                    {
                        var results = searcher.FindAll().Take(50); // Limit to 50 results
                        foreach (Principal result in results)
                        {
                            if (result is UserPrincipal up)
                            {
                                var user = MapUserPrincipalToADUser(up);
                                if (user != null)
                                    users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching users: {ex.Message}");
            }

            return users;
        }

        public List<ADUser> GetITSupportUsers()
        {
            var users = new List<ADUser>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var groupPrincipal = GroupPrincipal.FindByIdentity(context, _itSupportGroup))
                {
                    if (groupPrincipal != null)
                    {
                        var members = groupPrincipal.GetMembers();
                        foreach (Principal member in members)
                        {
                            if (member is UserPrincipal up)
                            {
                                var user = MapUserPrincipalToADUser(up);
                                if (user != null)
                                    users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting IT support users: {ex.Message}");
            }

            return users;
        }

        public bool IsUserInITSupportGroup(string username)
        {
            try
            {
                // Remove domain prefix if present
                if (username.Contains("\\"))
                {
                    username = username.Split('\\')[1];
                }

                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
                using (var groupPrincipal = GroupPrincipal.FindByIdentity(context, _itSupportGroup))
                {
                    if (userPrincipal == null || groupPrincipal == null)
                        return false;

                    return userPrincipal.IsMemberOf(groupPrincipal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking IT support group membership: {ex.Message}");
                return false;
            }
        }

        public List<string> GetUserGroups(string username)
        {
            var groups = new List<string>();

            try
            {
                // Remove domain prefix if present
                if (username.Contains("\\"))
                {
                    username = username.Split('\\')[1];
                }

                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
                {
                    if (userPrincipal != null)
                    {
                        var authGroups = userPrincipal.GetAuthorizationGroups();
                        foreach (Principal group in authGroups)
                        {
                            groups.Add(group.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user groups: {ex.Message}");
            }

            return groups;
        }

        private ADUser? MapUserPrincipalToADUser(UserPrincipal userPrincipal)
        {
            if (userPrincipal == null)
                return null;

            try
            {
                // Get the underlying DirectoryEntry for additional properties
                DirectoryEntry? directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;

                return new ADUser
                {
                    Username = userPrincipal.SamAccountName ?? string.Empty,
                    DisplayName = userPrincipal.DisplayName ?? userPrincipal.Name ?? string.Empty,
                    Email = userPrincipal.EmailAddress ?? string.Empty,
                    PhoneNumber = GetProperty(directoryEntry, "telephoneNumber"),
                    MobilePhone = GetProperty(directoryEntry, "mobile"),
                    Department = GetProperty(directoryEntry, "department"),
                    Title = GetProperty(directoryEntry, "title"),
                    Groups = GetUserGroups(userPrincipal.SamAccountName ?? string.Empty)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mapping user principal: {ex.Message}");
                return null;
            }
        }

        public List<string> SearchComputers(string searchTerm)
        {
            var computers = new List<string>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var computerPrincipal = new ComputerPrincipal(context))
                {
                    computerPrincipal.Name = $"*{searchTerm}*";
                    using (var searcher = new PrincipalSearcher(computerPrincipal))
                    {
                        var results = searcher.FindAll().Take(50); // Limit to 50 results
                        foreach (Principal result in results)
                        {
                            if (result is ComputerPrincipal cp && !string.IsNullOrEmpty(cp.Name))
                            {
                                computers.Add(cp.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching computers: {ex.Message}");
            }

            return computers.OrderBy(c => c).ToList();
        }

        private string? GetProperty(DirectoryEntry? entry, string propertyName)
        {
            if (entry == null || !entry.Properties.Contains(propertyName))
                return null;

            var value = entry.Properties[propertyName].Value;
            return value?.ToString();
        }
    }
}
