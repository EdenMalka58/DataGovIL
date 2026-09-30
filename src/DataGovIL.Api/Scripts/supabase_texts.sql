-- Run once in the Supabase SQL editor (or via psql) on a fresh database.
-- For an existing database use supabase_texts_v5.sql, which also moves the data.
--
-- Lookup texts referenced by id from public.manufacturer_models. Filled by the export.
--   table_id 1  drive             (hanaa_cd / hanaa_nm)                -> manufacturer_models.drive_code
--   table_id 2  fuel              (delek_cd / delek_nm)                -> manufacturer_models.fuel_code
--   table_id 3  homologation type (sug_tkina_cd / sug_tkina_nm)        -> manufacturer_models.homologation_type_code
--   table_id 4  converter type    (sug_mamir_cd / sug_mamir_nm)        -> manufacturer_models.converter_type_code
--   table_id 5  drive technology  (technologiat_hanaa_cd / _nm)        -> manufacturer_models.drive_technology_code
--   table_id 6  body type         (merkav; ids assigned 1, 2, 3, ...)  -> manufacturer_models.body_type
-- A name with no source code is stored with id 0.

create table if not exists public.texts (
  table_id integer not null,
  id       integer not null,
  text     text    not null,
  constraint texts_pkey primary key (table_id, id)
);

comment on table public.texts is
  'Lookup texts referenced by id from manufacturer_models (see table_id legend in supabase_texts.sql)';
