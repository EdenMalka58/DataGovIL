using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the "תוצרים ודגמים של כלי רכב WLTP" datastore resource.
/// Used only to deserialize CKAN JSON, whose keys are the live datastore field ids.
/// Types from data.gov.il (int / numeric / text) are stored as <see cref="string"/> so
/// CKAN's inconsistent JSON typing still deserializes via <c>FlexibleStringConverter</c>.
/// Unmapped columns still arrive via <see cref="ExtensionData"/>.
/// Map to <see cref="ManufacturerModelRecord"/> before returning from the API.
/// </summary>
public class ManufacturerModelDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("sug_degem")]
    public string? ModelType { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("tozeret_eretz_nm")]
    public string? ManufacturerCountryName { get; set; }

    [JsonPropertyName("tozar")]
    public string? Tozar { get; set; }

    [JsonPropertyName("degem_cd")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ModelYear { get; set; }

    [JsonPropertyName("kvuzat_agra_cd")]
    public string? FeeGroupCode { get; set; }

    [JsonPropertyName("nefah_manoa")]
    public string? EngineDisplacement { get; set; }

    [JsonPropertyName("mishkal_kolel")]
    public string? TotalWeight { get; set; }

    [JsonPropertyName("gova")]
    public string? Height { get; set; }

    [JsonPropertyName("hanaa_cd")]
    public string? DriveCode { get; set; }

    [JsonPropertyName("hanaa_nm")]
    public string? DriveName { get; set; }

    [JsonPropertyName("mazgan_ind")]
    public string? AirConditioningIndicator { get; set; }

    [JsonPropertyName("abs_ind")]
    public string? AbsIndicator { get; set; }

    [JsonPropertyName("kariot_avir_source")]
    public string? AirbagsSource { get; set; }

    [JsonPropertyName("mispar_kariot_avir")]
    public string? AirbagCount { get; set; }

    [JsonPropertyName("hege_koah_ind")]
    public string? PowerSteeringIndicator { get; set; }

    [JsonPropertyName("automatic_ind")]
    public string? AutomaticTransmissionIndicator { get; set; }

    [JsonPropertyName("halonot_hashmal_source")]
    public string? PowerWindowsSource { get; set; }

    [JsonPropertyName("mispar_halonot_hashmal")]
    public string? PowerWindowCount { get; set; }

    [JsonPropertyName("halon_bagg_ind")]
    public string? PowerSunroofIndicator { get; set; }

    [JsonPropertyName("galgaley_sagsoget_kala_ind")]
    public string? AlloyWheelsIndicator { get; set; }

    [JsonPropertyName("argaz_ind")]
    public string? CargoBoxIndicator { get; set; }

    [JsonPropertyName("merkav")]
    public string? BodyType { get; set; }

    [JsonPropertyName("ramat_gimur")]
    public string? TrimLevel { get; set; }

    [JsonPropertyName("delek_cd")]
    public string? FuelCode { get; set; }

    [JsonPropertyName("delek_nm")]
    public string? FuelName { get; set; }

    [JsonPropertyName("mispar_dlatot")]
    public string? DoorCount { get; set; }

    [JsonPropertyName("koah_sus")]
    public string? Horsepower { get; set; }

    [JsonPropertyName("mispar_moshavim")]
    public string? SeatCount { get; set; }

    [JsonPropertyName("bakarat_yatzivut_ind")]
    public string? StabilityControlIndicator { get; set; }

    [JsonPropertyName("kosher_grira_im_blamim")]
    public string? TowingCapacityWithBrakes { get; set; }

    [JsonPropertyName("kosher_grira_bli_blamim")]
    public string? TowingCapacityWithoutBrakes { get; set; }

    [JsonPropertyName("sug_tkina_cd")]
    public string? HomologationTypeCode { get; set; }

    [JsonPropertyName("sug_tkina_nm")]
    public string? HomologationTypeName { get; set; }

    [JsonPropertyName("sug_mamir_cd")]
    public string? ConverterTypeCode { get; set; }

    [JsonPropertyName("sug_mamir_nm")]
    public string? ConverterTypeName { get; set; }

    [JsonPropertyName("technologiat_hanaa_cd")]
    public string? DriveTechnologyCode { get; set; }

    [JsonPropertyName("technologiat_hanaa_nm")]
    public string? DriveTechnologyName { get; set; }

    [JsonPropertyName("kamut_CO2")]
    public string? Co2Amount { get; set; }

    [JsonPropertyName("kamut_NOX")]
    public string? NoxAmount { get; set; }

    [JsonPropertyName("kamut_PM10")]
    public string? Pm10Amount { get; set; }

    [JsonPropertyName("kamut_HC")]
    public string? HcAmount { get; set; }

    [JsonPropertyName("kamut_HC_NOX")]
    public string? HcNoxAmount { get; set; }

    [JsonPropertyName("kamut_CO")]
    public string? CoAmount { get; set; }

    [JsonPropertyName("kamut_CO2_city")]
    public string? Co2AmountCity { get; set; }

    [JsonPropertyName("kamut_NOX_city")]
    public string? NoxAmountCity { get; set; }

    [JsonPropertyName("kamut_PM10_city")]
    public string? Pm10AmountCity { get; set; }

    [JsonPropertyName("kamut_HC_city")]
    public string? HcAmountCity { get; set; }

    [JsonPropertyName("kamut_CO_city")]
    public string? CoAmountCity { get; set; }

    [JsonPropertyName("kamut_CO2_hway")]
    public string? Co2AmountHighway { get; set; }

    [JsonPropertyName("kamut_NOX_hway")]
    public string? NoxAmountHighway { get; set; }

    [JsonPropertyName("kamut_PM10_hway")]
    public string? Pm10AmountHighway { get; set; }

    [JsonPropertyName("kamut_HC_hway")]
    public string? HcAmountHighway { get; set; }

    [JsonPropertyName("kamut_CO_hway")]
    public string? CoAmountHighway { get; set; }

    [JsonPropertyName("madad_yarok")]
    public string? GreenScore { get; set; }

    [JsonPropertyName("kvutzat_zihum")]
    public string? PollutionGroup { get; set; }

    [JsonPropertyName("bakarat_stiya_menativ_ind")]
    public string? LaneDepartureControlIndicator { get; set; }

    [JsonPropertyName("bakarat_stiya_menativ_makor_hatkana")]
    public string? LaneDepartureControlRegulationSource { get; set; }

    [JsonPropertyName("nitur_merhak_milfanim_ind")]
    public string? ForwardDistanceMonitoringIndicator { get; set; }

    [JsonPropertyName("nitur_merhak_milfanim_makor_hatkana")]
    public string? ForwardDistanceMonitoringRegulationSource { get; set; }

    [JsonPropertyName("zihuy_beshetah_nistar_ind")]
    public string? BlindSpotDetectionIndicator { get; set; }

    [JsonPropertyName("bakarat_shyut_adaptivit_ind")]
    public string? AdaptiveCruiseControlIndicator { get; set; }

    [JsonPropertyName("zihuy_holchey_regel_ind")]
    public string? PedestrianDetectionIndicator { get; set; }

    [JsonPropertyName("zihuy_holchey_regel_makor_hatkana")]
    public string? PedestrianDetectionRegulationSource { get; set; }

    [JsonPropertyName("maarechet_ezer_labalam_ind")]
    public string? BrakeAssistIndicator { get; set; }

    [JsonPropertyName("matzlemat_reverse_ind")]
    public string? ReverseCameraIndicator { get; set; }

    [JsonPropertyName("hayshaney_lahatz_avir_batzmigim_ind")]
    public string? TirePressureMonitoringIndicator { get; set; }

    [JsonPropertyName("hayshaney_hagorot_ind")]
    public string? SeatbeltReminderIndicator { get; set; }

    [JsonPropertyName("nikud_betihut")]
    public string? SafetyScore { get; set; }

    [JsonPropertyName("ramat_eivzur_betihuty")]
    public string? SafetyEquipmentLevel { get; set; }

    [JsonPropertyName("teura_automatit_benesiya_kadima_ind")]
    public string? AutomaticLightingForwardTravelIndicator { get; set; }

    [JsonPropertyName("shlita_automatit_beorot_gvohim_ind")]
    public string? AutomaticHighBeamControlIndicator { get; set; }

    [JsonPropertyName("shlita_automatit_beorot_gvohim_makor_hatkana")]
    public string? AutomaticHighBeamControlRegulationSource { get; set; }

    [JsonPropertyName("zihuy_matzav_hitkarvut_mesukenet_ind")]
    public string? DangerousApproachDetectionIndicator { get; set; }

    [JsonPropertyName("zihuy_tamrurey_tnua_ind")]
    public string? TrafficSignRecognitionIndicator { get; set; }

    [JsonPropertyName("zihuy_rechev_do_galgali")]
    public string? TwoWheeledVehicleDetection { get; set; }

    [JsonPropertyName("zihuy_tamrurey_tnua_makor_hatkana")]
    public string? TrafficSignRecognitionRegulationSource { get; set; }

    [JsonPropertyName("CO2_WLTP")]
    public string? Co2Wltp { get; set; }

    [JsonPropertyName("HC_WLTP")]
    public string? HcWltp { get; set; }

    [JsonPropertyName("PM_WLTP")]
    public string? PmWltp { get; set; }

    [JsonPropertyName("NOX_WLTP")]
    public string? NoxWltp { get; set; }

    [JsonPropertyName("CO_WLTP")]
    public string? CoWltp { get; set; }

    [JsonPropertyName("CO2_WLTP_NEDC")]
    public string? Co2WltpNedc { get; set; }

    [JsonPropertyName("bakarat_stiya_activ_s")]
    public string? ActiveLaneControl { get; set; }

    [JsonPropertyName("blima_otomatit_nesia_leahor")]
    public string? AutomaticBrakingReverseTravel { get; set; }

    [JsonPropertyName("bakarat_mehirut_isa")]
    public string? SpeedControlIsa { get; set; }

    [JsonPropertyName("blimat_hirum_lifnei_holhei_regel_ofanaim")]
    public string? EmergencyBrakingPedestriansCyclists { get; set; }

    [JsonPropertyName("hitnagshut_cad_shetah_met")]
    public string? SideCollisionBlindSpot { get; set; }

    [JsonPropertyName("alco_lock")]
    public string? AlcoLock { get; set; }

    [JsonPropertyName("dg_metach_solela")]
    public string? BatteryVoltageDg { get; set; }

    [JsonPropertyName("kinuy_mishari")]
    public string? CommercialName { get; set; }

    /// <summary>Any column not mapped above is preserved here, keyed by the raw datastore field id.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
