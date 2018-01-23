using static GooglePlayStoreApi.EnumExtensions;

namespace GooglePlayStoreApi.Model
{
    public enum CountryCode
    {
        [Country("bg"), CountryCode("bg-BG")]
        Bulgarian,

        [Country("cn"), CountryCode("zh-CN")]
        China,

        [Country("tw"), CountryCode("zh-TW")]
        Taiwan,

        [Country("cz"), CountryCode("cs-CZ")]
        Czech,

        [Country("dk"), CountryCode("da-DK")]
        Denmark,

        [Country("nl"), CountryCode("nl-NL")]
        Netherlands,

        [Country("uk"), CountryCode("en-UK")]
        UnitedKingdom,

        [Country("us"), CountryCode("en-US")]
        UnitedStates,

        [Country("ph"), CountryCode("tl-PH")]
        Philippines,

        [Country("fi"), CountryCode("fi-FI")]
        Finland,

        [Country("fr"), CountryCode("fr-FR")]
        France,

        [Country("de"), CountryCode("de-DE")]
        Germany,

        [Country("gr"), CountryCode("el-GR")]
        Greece,

        [Country("hu"), CountryCode("hu-HU")]
        Hungary,

        [Country("id"), CountryCode("id-ID")]
        Indonesia,

        [Country("it"), CountryCode("it-IT")]
        Italy,

        [Country("jp"), CountryCode("ja-JP")]
        Japan,

        [Country("kr"), CountryCode("ko-KR")]
        Korea,

        [Country("lt"), CountryCode("lt-LT")]
        Lithuania,

        [Country("no"), CountryCode("no-No")]
        Norway,

        [Country("pl"), CountryCode("pl-PL")]
        Poland,

        [Country("br"), CountryCode("pt-BR")]
        Brazil,

        [Country("pt"), CountryCode("pt-PT")]
        Portugal,

        [Country("ru"), CountryCode("ru")]
        Russia,

        [Country("sk"), CountryCode("sk-SK")]
        Slovakia,

        [Country("es"), CountryCode("es-ES")]
        Spain,

        [Country("se"), CountryCode("sv-SE")]
        Sweden,

        [Country("th"), CountryCode("th-TH")]
        Thailand,

        [Country("tr"), CountryCode("tr-TR")]
        Turkey,

        [Country("vi"), CountryCode("vi-VN")]
        Vietnam
    }
}
