﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.Core.Domain
{
    public class OrganizationUsageLimit
    {
        public int OrganizationId { get; set; }
        public string LicensingType { get; set; }
        public string BillingType { get; set; }

        public long? NumberOfStorageBytes { get; set; }
        public double? TranslationMinutes { get; set; }
        public double? TranslationMinutesLimit { get; set; }


        public long ConsumedNumberOfStorageBytes { get; set; }
        public double ConsumedTranslationMinutes { get; set; }

        public long? RemainingNumberOfStorageBytes { get; set; }
        public double? RemainingTranslationMinutes { get; set; }

        public bool TranslationLimitExceeded { get; set; } = false;
        public bool StorageLimitExceeded { get; set; } = false;
    }
}
