using System;
using System.Text;

namespace RulesOfEntry.Campaign
{
    public static class CampaignDataRules
    {
        public const int CurrentSchemaVersion = 1;
        public const int MaximumOfficerNameLength = 48;
        public const int MaximumBadgeLength = 12;
        public const string DefaultDepartmentName =
            "Calder City Police Department";

        public static string NormalizeOfficerName(string value)
        {
            return NormalizeCharacters(
                value,
                MaximumOfficerNameLength,
                character => char.IsLetter(character)
                    || character == ' '
                    || character == '\''
                    || character == '-'
                    || character == '.');
        }

        public static string NormalizeBadgeIdentifier(string value)
        {
            string normalized = NormalizeCharacters(
                value,
                MaximumBadgeLength,
                character => char.IsLetterOrDigit(character)
                    || character == '-');
            return normalized.ToUpperInvariant();
        }

        public static string NormalizeDepartmentName(string value)
        {
            string normalized = NormalizeCharacters(
                value,
                72,
                character => char.IsLetterOrDigit(character)
                    || character == ' '
                    || character == '&'
                    || character == '-'
                    || character == '.');
            return string.IsNullOrWhiteSpace(normalized)
                ? DefaultDepartmentName
                : normalized;
        }

        public static bool TryValidateNewCampaign(
            string officerName,
            string badgeIdentifier,
            out string normalizedOfficerName,
            out string normalizedBadgeIdentifier,
            out string error)
        {
            normalizedOfficerName = NormalizeOfficerName(officerName);
            normalizedBadgeIdentifier = NormalizeBadgeIdentifier(badgeIdentifier);
            if (normalizedOfficerName.Length < 2)
            {
                error = "Enter the officer's full name.";
                return false;
            }

            if (normalizedBadgeIdentifier.Length < 2)
            {
                error = "Enter a valid department badge number.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static int WrapArchiveIndex(int requestedIndex, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            int wrapped = requestedIndex % count;
            return wrapped < 0 ? wrapped + count : wrapped;
        }

        public static bool IsValidCampaignId(string value)
        {
            return Guid.TryParseExact(value, "N", out _);
        }

        private static string NormalizeCharacters(
            string value,
            int maximumLength,
            Func<char, bool> isAllowed)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(
                Math.Min(value.Length, maximumLength));
            bool previousWasSpace = true;
            foreach (char character in value.Trim())
            {
                if (builder.Length >= maximumLength)
                {
                    break;
                }

                if (!isAllowed(character))
                {
                    continue;
                }

                bool isSpace = character == ' ';
                if (isSpace && previousWasSpace)
                {
                    continue;
                }

                builder.Append(character);
                previousWasSpace = isSpace;
            }

            return builder.ToString().Trim();
        }
    }
}
