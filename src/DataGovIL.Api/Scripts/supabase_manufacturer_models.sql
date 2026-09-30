-- Run once in the Supabase SQL editor (or via psql) on a fresh database.
-- For an existing table apply supabase_manufacturer_models_alter_v2.sql, _alter_v3.sql,
-- supabase_sync_content_hash_v4.sql, then supabase_texts_v5.sql.
-- Text columns (drive, fuel, homologation, converter, drive technology, body type) hold
-- ids into public.texts — create it with supabase_texts.sql.
--
-- Source: WLTP resource 142afde2-6228-49f9-8a29-9b6c3a0cbe40 (all fields, English names)
-- joined with vehicle counts resource 5e87a7a1-2f6f-41c1-8aec-7216d52a6cf6.
-- Manufacturer name / brand / country are not stored here: left join public.manufacturers
-- on manufacturer_code (a few WLTP codes are missing from that list).

create table if not exists public.manufacturer_models (
  manufacturer_code                    integer     not null, -- tozeret_cd
  model_code                           integer     not null, -- degem_cd
  model_year                           integer     not null, -- shnat_yitzur
  model_type                           text        not null, -- sug_degem
  model_name                           text,                 -- degem_nm
  commercial_name                      text,                 -- kinuy_mishari
  trim_level                           text,                 -- ramat_gimur
  body_type                            integer,              -- merkav -> texts id (table_id 6)
  fee_group_code                       numeric,              -- kvuzat_agra_cd
  engine_displacement                  numeric,              -- nefah_manoa
  total_weight                         numeric,              -- mishkal_kolel
  height                               numeric,              -- gova
  drive_code                           integer,              -- hanaa_cd -> texts id (table_id 1)
  drive_technology_code                integer,              -- technologiat_hanaa_cd -> texts id (table_id 5)
  fuel_code                            integer,              -- delek_cd -> texts id (table_id 2)
  horsepower                           numeric,              -- koah_sus
  door_count                           numeric,              -- mispar_dlatot
  seat_count                           numeric,              -- mispar_moshavim
  towing_capacity_braked               numeric,              -- kosher_grira_im_blamim
  towing_capacity_unbraked             numeric,              -- kosher_grira_bli_blamim
  homologation_type_code               integer,              -- sug_tkina_cd -> texts id (table_id 3)
  converter_type_code                  integer,              -- sug_mamir_cd -> texts id (table_id 4)
  air_conditioning_ind                 numeric,              -- mazgan_ind
  abs_ind                              numeric,              -- abs_ind
  airbags_source                       text,                 -- kariot_avir_source
  airbag_count                         numeric,              -- mispar_kariot_avir
  power_steering_ind                   numeric,              -- hege_koah_ind
  automatic_transmission_ind           numeric,              -- automatic_ind
  power_windows_source                 text,                 -- halonot_hashmal_source
  power_window_count                   numeric,              -- mispar_halonot_hashmal
  sunroof_ind                          numeric,              -- halon_bagg_ind
  alloy_wheels_ind                     numeric,              -- galgaley_sagsoget_kala_ind
  cargo_box_ind                        numeric,              -- argaz_ind
  stability_control_ind                numeric,              -- bakarat_yatzivut_ind
  co2                                  numeric,              -- kamut_CO2
  nox                                  numeric,              -- kamut_NOX
  pm10                                 numeric,              -- kamut_PM10
  hc                                   numeric,              -- kamut_HC
  hc_nox                               numeric,              -- kamut_HC_NOX
  co                                   numeric,              -- kamut_CO
  co2_city                             numeric,              -- kamut_CO2_city
  nox_city                             numeric,              -- kamut_NOX_city
  pm10_city                            numeric,              -- kamut_PM10_city
  hc_city                              numeric,              -- kamut_HC_city
  co_city                              numeric,              -- kamut_CO_city
  co2_highway                          numeric,              -- kamut_CO2_hway
  nox_highway                          numeric,              -- kamut_NOX_hway
  pm10_highway                         numeric,              -- kamut_PM10_hway
  hc_highway                           numeric,              -- kamut_HC_hway
  co_highway                           numeric,              -- kamut_CO_hway
  co2_wltp                             numeric,              -- CO2_WLTP
  hc_wltp                              numeric,              -- HC_WLTP
  pm_wltp                              numeric,              -- PM_WLTP
  nox_wltp                             numeric,              -- NOX_WLTP
  co_wltp                              numeric,              -- CO_WLTP
  co2_wltp_nedc                        numeric,              -- CO2_WLTP_NEDC
  green_index                          numeric,              -- madad_yarok
  pollution_group                      numeric,              -- kvutzat_zihum
  safety_score                         numeric,              -- nikud_betihut
  safety_equipment_level               numeric,              -- ramat_eivzur_betihuty
  lane_departure_control_ind           numeric,              -- bakarat_stiya_menativ_ind
  lane_departure_control_source        text,                 -- bakarat_stiya_menativ_makor_hatkana
  forward_distance_monitoring_ind      numeric,              -- nitur_merhak_milfanim_ind
  forward_distance_monitoring_source   text,                 -- nitur_merhak_milfanim_makor_hatkana
  blind_spot_detection_ind             numeric,              -- zihuy_beshetah_nistar_ind
  adaptive_cruise_control_ind          numeric,              -- bakarat_shyut_adaptivit_ind
  pedestrian_detection_ind             numeric,              -- zihuy_holchey_regel_ind
  pedestrian_detection_source          text,                 -- zihuy_holchey_regel_makor_hatkana
  brake_assist_ind                     numeric,              -- maarechet_ezer_labalam_ind
  reverse_camera_ind                   numeric,              -- matzlemat_reverse_ind
  tire_pressure_sensors_ind            numeric,              -- hayshaney_lahatz_avir_batzmigim_ind
  seatbelt_sensors_ind                 numeric,              -- hayshaney_hagorot_ind
  auto_headlights_ind                  numeric,              -- teura_automatit_benesiya_kadima_ind
  auto_high_beam_ind                   numeric,              -- shlita_automatit_beorot_gvohim_ind
  auto_high_beam_source                text,                 -- shlita_automatit_beorot_gvohim_makor_hatkana
  dangerous_approach_detection_ind     numeric,              -- zihuy_matzav_hitkarvut_mesukenet_ind
  traffic_sign_recognition_ind         numeric,              -- zihuy_tamrurey_tnua_ind
  traffic_sign_recognition_source      text,                 -- zihuy_tamrurey_tnua_makor_hatkana
  two_wheeler_detection                numeric,              -- zihuy_rechev_do_galgali
  active_lane_keeping                  numeric,              -- bakarat_stiya_activ_s
  reverse_auto_braking                 numeric,              -- blima_otomatit_nesia_leahor
  intelligent_speed_assist             numeric,              -- bakarat_mehirut_isa
  pedestrian_cyclist_emergency_braking numeric,              -- blimat_hirum_lifnei_holhei_regel_ofanaim
  blind_spot_side_collision            numeric,              -- hitnagshut_cad_shetah_met
  alcohol_interlock                    numeric,              -- alco_lock
  battery_voltage_class                numeric,              -- dg_metach_solela
  active_vehicle_count                 numeric,              -- mispar_rechavim_pailim (counts resource)
  inactive_vehicle_count               numeric,              -- mispar_rechavim_le_pailim (counts resource)
  content_hash                         text,                 -- hash of all non-key columns (delta sync)
  updated_at                           timestamptz not null default now(),
  constraint manufacturer_models_pkey
    primary key (manufacturer_code, model_code, model_year, model_type)
);

comment on table public.manufacturer_models is
  'WLTP make/model/year catalog synced from data.gov.il, with active/inactive vehicle counts';
