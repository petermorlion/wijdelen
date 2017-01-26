using System;

namespace WijDelen.Reports {
    public class DateTimeProvider : IDateTimeProvider {
        public DateTime UtcNow() {
            return DateTime.UtcNow;
        }
    }
}