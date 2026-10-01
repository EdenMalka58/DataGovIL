using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>The data.gov.il registry a vehicle lookup was answered from.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleDataSource
{
    /// <summary>כלי רכב פרטיים ומסחריים (both resource parts).</summary>
    PrivateAndCommercial,

    /// <summary>כלי הרכב הציבוריים הפעילים: taxis, shared taxis, buses.</summary>
    PublicTransport,

    /// <summary>כלי רכב מעל 3.5 טון וכלי רכב חסרי קוד דגם: trucks, tractors, trailers, work vehicles, buses.</summary>
    HeavyOrNoModelCode,

    /// <summary>כלי רכב דו גלגליים: motorcycles and scooters.</summary>
    TwoWheeled,

    /// <summary>ביטול סופי (current and historical resources).</summary>
    PermanentlyCancelled,

    /// <summary>כלי רכב לא פעילים (with or without a model code).</summary>
    Inactive,

    /// <summary>כלי רכב ביבוא אישי.</summary>
    PersonalImport
}
