// MadeByUserDto.cs
// Author: Ondřej Ondryáš

namespace KachnaOnline.Dto.Users
{
    public class UserDetailsDto : UserDto
    {
        /// <summary>
        /// Email of the user.
        /// </summary>
        /// <example>foo@bar.cz</example>
        public string Email { get; set; }

        /// <summary>
        /// Optional Discord ID of the user.
        /// </summary>
        /// <example>4378291</example>
        public ulong? DiscordId { get; set; }

        /// <summary>
        /// Roles that the user has.
        /// </summary>
        /// <example>["BoardGamesManager", "Admin"]</example>
        public string[] ActiveRoles { get; set; }

        /// <summary>
        /// Detailed information about the user's role assignments.
        /// </summary>
        public UserRoleAssignmentDetailsDto[] ManuallyAssignedRoles { get; set; }
    }
}
