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
            if (user.Avatar == null) user.Avatar = InputConstraintsAndDefaultValues.DefaultUserAvatar;

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
                        user.Name.Length        >= InputConstraintsAndDefaultValues.MinUserNameLength      &&
                        user.Name.Length        <= InputConstraintsAndDefaultValues.MaxUserNameLength      &&
                        user.LastName.Length    >= InputConstraintsAndDefaultValues.MinUserLastNameLength  &&
                        user.LastName.Length    <= InputConstraintsAndDefaultValues.MaxUserLastNameLength  &&
                        user.Email.Length       >= InputConstraintsAndDefaultValues.MinUserEmailLength     &&
                        user.Email.Length       <= InputConstraintsAndDefaultValues.MaxUserEmailLength     &&
                        user.Password.Length    >= InputConstraintsAndDefaultValues.MinUserPasswordLength  &&
                        user.Password.Length    <= InputConstraintsAndDefaultValues.MaxUserPasswordLength  &&
                        user.Avatar.Length      <= InputConstraintsAndDefaultValues.MaxImageSize
                    );
        }
    }
}
