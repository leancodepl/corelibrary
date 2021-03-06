﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Xunit;

namespace System._Time.Tests
{
    public class DateFormattingTests
    {
        public DateFormattingTests()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        [Fact]
        public void ToLongDateString()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToLongDateString();
            Assert.Equal("Sunday, 31 December 2000", s);
        }

        [Fact]
        public void ToLongDateStringInvariant()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToLongDateStringInvariant();
            Assert.Equal("Sunday, 31 December 2000", s);
        }

        [Fact]
        public void ToShortDateString()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToShortDateString();
            Assert.Equal("12/31/2000", s);
        }

        [Fact]
        public void ToShortDateStringInvariant()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToShortDateStringInvariant();
            Assert.Equal("12/31/2000", s);
        }

        [Fact]
        public void ToIsoString()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToIsoString();
            Assert.Equal("2000-12-31", s);
        }

        [Fact]
        public void ToStringWithStandardDateFormat()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString("d");
            Assert.Equal("12/31/2000", s);
        }

        [Fact]
        public void ToStringWithNullDateFormat()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString((string?)null);
            Assert.Equal("12/31/2000", s);
        }

        [Fact]
        public void ToStringWithEmptyDateFormat()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString(string.Empty);
            Assert.Equal("12/31/2000", s);
        }

        [Fact]
        public void ToStringWithCustomDateFormat()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString("dd MMM yyyy");
            Assert.Equal("31 Dec 2000", s);
        }

        [Fact]
        public void ToStringWithISODateFormat_O()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString("O");
            Assert.Equal("2000-12-31", s);
        }

        [Fact]
        public void ToStringWithISODateFormat_o()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString("o");
            Assert.Equal("2000-12-31", s);
        }

        [Fact]
        public void ToStringWithISODateFormat_s()
        {
            var date = new Date(2000, 12, 31);
            var s = date.ToString("s");
            Assert.Equal("2000-12-31", s);
        }

        [Fact]
        public void ToStringWithCustomDateTimeFormat()
        {
            Assert.Throws<FormatException>(() => new Date(2000, 12, 31).ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [Fact]
        public void ToStringWithStandardTimeFormat()
        {
            Assert.Throws<FormatException>(() => new Date(2000, 12, 31).ToString("t"));
        }

        [Fact]
        public void ToStringWithCustomTimeFormat()
        {
            Assert.Throws<FormatException>(() => new Date(2000, 12, 31).ToString("HH:mm:ss"));
        }
    }
}
