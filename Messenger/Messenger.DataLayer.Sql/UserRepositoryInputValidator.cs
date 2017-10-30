using Messenger.Model;

namespace Messenger.DataLayer.Sql
{
    class UserRepositoryInputValidator
    {
        public bool ValidateCreate(User user)
        {
            return  (
                        ValidateCreate_CheckNull(user) && 
                        ValidateCreate_CheckRange(user)
                    );
        }

        private bool ValidateCreate_CheckNull(User user)
        {
            if (user == null) return false;
            if (user.Avatar == null) user.Avatar = InputRestrictionsAndDefaultValues.DefaultUserAvatar;

            return  (
                        user.Name       != null &&
                        user.LastName   != null &&
                        user.Email      != null &&
                        user.Password   != null
                    );
        }

        private bool ValidateCreate_CheckRange(User user)
        {
            return  (
                        user.Name.Length        >= InputRestrictionsAndDefaultValues.MinUserNameLength      &&
                        user.Name.Length        <= InputRestrictionsAndDefaultValues.MaxUserNameLength      &&
                        user.LastName.Length    >= InputRestrictionsAndDefaultValues.MinUserLastNameLength  &&
                        user.LastName.Length    <= InputRestrictionsAndDefaultValues.MaxUserLastNameLength  &&
                        user.Email.Length       >= InputRestrictionsAndDefaultValues.MinUserEmailLength     &&
                        user.Email.Length       <= InputRestrictionsAndDefaultValues.MaxUserEmailLength     &&
                        user.Password.Length    >= InputRestrictionsAndDefaultValues.MinUserPasswordLength  &&
                        user.Password.Length    <= InputRestrictionsAndDefaultValues.MaxUserPasswordLength
                    );
        }
    }
}
