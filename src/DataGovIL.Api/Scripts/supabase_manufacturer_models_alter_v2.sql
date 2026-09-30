-- Upgrades public.manufacturer_models from the v1 shape (one row per tozeret_cd + degem_cd,
-- Hebrew column names, "years" range) to v2: every WLTP field with English column names,
-- one row per (manufacturer_code, model_code, model_year, model_type), plus vehicle counts.
--
-- The row grain changes, so existing rows cannot be converted: the table is emptied and the
-- next export repopulates it. Run once in the Supabase SQL editor.

begin;

truncate table public.manufacturer_models;

alter table public.manufacturer_models drop constraint if exists manufacturer_models_pkey;
drop index if exists public.manufacturer_models_tozeret_nm_idx;

alter table public.manufacturer_models rename column tozeret_cd    to manufacturer_code;
alter table public.manufacturer_models rename column tozeret_nm    to manufacturer_name;
alter table public.manufacturer_models rename column tozar         to brand;
alter table public.manufacturer_models rename column degem_cd      to model_code;
alter table public.manufacturer_models rename column degem_nm      to model_name;
alter table public.manufacturer_models rename column kinuy_mishari to commercial_name;
alter table public.manufacturer_models rename column ramat_gimur   to trim_level;

alter table public.manufacturer_models drop column if exists years;

alter table public.manufacturer_models
  alter column manufacturer_code type integer using manufacturer_code::integer,
  alter column model_code        type integer using model_code::integer;

alter table public.manufacturer_models
  add column model_year                           integer not null,
  add column model_type                           text    not null,
  add column manufacturer_country                 text,
  add column body_type                            text,
  add column fee_group_code                       numeric,
  add column engine_displacement                  numeric,
  add column total_weight                         numeric,
  add column height                               numeric,
  add column drive_code                           numeric,
  add column drive_name                           text,
  add column drive_technology_code                numeric,
  add column drive_technology_name                text,
  add column fuel_code                            numeric,
  add column fuel_name                            text,
  add column horsepower                           numeric,
  add column door_count                           numeric,
  add column seat_count                           numeric,
  add column towing_capacity_braked               numeric,
  add column towing_capacity_unbraked             numeric,
  add column homologation_type_code               numeric,
  add column homologation_type_name               text,
  add column converter_type_code                  numeric,
  add column converter_type_name                  text,
  add column air_conditioning_ind                 numeric,
  add column abs_ind                              numeric,
  add column airbags_source                       text,
  add column airbag_count                         numeric,
  add column power_steering_ind                   numeric,
  add column automatic_transmission_ind           numeric,
  add column power_windows_source                 text,
  add column power_window_count                   numeric,
  add column sunroof_ind                          numeric,
  add column alloy_wheels_ind                     numeric,
  add column cargo_box_ind                        numeric,
  add column stability_control_ind                numeric,
  add column co2                                  numeric,
  add column nox                                  numeric,
  add column pm10                                 numeric,
  add column hc                                   numeric,
  add column hc_nox                               numeric,
  add column co                                   numeric,
  add column co2_city                             numeric,
  add column nox_city                             numeric,
  add column pm10_city                            numeric,
  add column hc_city                              numeric,
  add column co_city                              numeric,
  add column co2_highway                          numeric,
  add column nox_highway                          numeric,
  add column pm10_highway                         numeric,
  add column hc_highway                           numeric,
  add column co_highway                           numeric,
  add column co2_wltp                             numeric,
  add column hc_wltp                              numeric,
  add column pm_wltp                              numeric,
  add column nox_wltp                             numeric,
  add column co_wltp                              numeric,
  add column co2_wltp_nedc                        numeric,
  add column green_index                          numeric,
  add column pollution_group                      numeric,
  add column safety_score                         numeric,
  add column safety_equipment_level               numeric,
  add column lane_departure_control_ind           numeric,
  add column lane_departure_control_source        text,
  add column forward_distance_monitoring_ind      numeric,
  add column forward_distance_monitoring_source   text,
  add column blind_spot_detection_ind             numeric,
  add column adaptive_cruise_control_ind          numeric,
  add column pedestrian_detection_ind             numeric,
  add column pedestrian_detection_source          text,
  add column brake_assist_ind                     numeric,
  add column reverse_camera_ind                   numeric,
  add column tire_pressure_sensors_ind            numeric,
  add column seatbelt_sensors_ind                 numeric,
  add column auto_headlights_ind                  numeric,
  add column auto_high_beam_ind                   numeric,
  add column auto_high_beam_source                text,
  add column dangerous_approach_detection_ind     numeric,
  add column traffic_sign_recognition_ind         numeric,
  add column traffic_sign_recognition_source      text,
  add column two_wheeler_detection                numeric,
  add column active_lane_keeping                  numeric,
  add column reverse_auto_braking                 numeric,
  add column intelligent_speed_assist             numeric,
  add column pedestrian_cyclist_emergency_braking numeric,
  add column blind_spot_side_collision            numeric,
  add column alcohol_interlock                    numeric,
  add column battery_voltage_class                numeric,
  add column active_vehicle_count                 numeric,
  add column inactive_vehicle_count               numeric;

alter table public.manufacturer_models
  add constraint manufacturer_models_pkey
  primary key (manufacturer_code, model_code, model_year, model_type);

create index if not exists manufacturer_models_manufacturer_name_idx
  on public.manufacturer_models (manufacturer_name);

comment on table public.manufacturer_models is
  'WLTP make/model/year catalog synced from data.gov.il, with active/inactive vehicle counts';

commit;
