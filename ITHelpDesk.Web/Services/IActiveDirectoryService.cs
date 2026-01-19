using ITHelpDesk.Web.Models;

namespace ITHelpDesk.Web.Services
{
    public interface IActiveDirectoryService
    {
        /// <summary>
        /// Gets user information from Active Directory by username
        /// </summary>
        ADUser? GetUserByUsername(string username);

        /// <summary>
        /// Gets user information from Active Directory by email
        /// </summary>
        ADUser? GetUserByEmail(string email);

        /// <summary>
        /// Searches for users in Active Directory
        /// </summary>
        List<ADUser> SearchUsers(string searchTerm);

        /// <summary>
        /// Gets all members of the IT Support group
        /// </summary>
        List<ADUser> GetITSupportUsers();

        /// <summary>
        /// Checks if a user is a member of the IT Support group
        /// </summary>
        bool IsUserInITSupportGroup(string username);

        /// <summary>
        /// Gets all groups a user is a member of
        /// </summary>
        List<string> GetUserGroups(string username);
    }
}
