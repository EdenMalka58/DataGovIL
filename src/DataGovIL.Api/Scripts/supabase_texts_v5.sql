-- Moves display texts out of public.manufacturer_models into a shared lookup table,
-- public.texts (table_id, id, text). manufacturer_models keeps only the id.
--
--   table_id 1  drive             hanaa_cd / hanaa_nm                     -> drive_code
--   table_id 2  fuel              delek_cd / delek_nm                     -> fuel_code
--   table_id 3  homologation type sug_tkina_cd / sug_tkina_nm             -> homologation_type_code
--   table_id 4  converter type    sug_mamir_cd / sug_mamir_nm             -> converter_type_code
--   table_id 5  drive technology  technologiat_hanaa_cd / _nm             -> drive_technology_code
--   table_id 6  body type         merkav (no source code; running id)     -> body_type
--
-- A name with no code (e.g. drive technology "הנעה רגילה") gets id 0, matching the export.
-- Run once.

begin;

create table if not exists public.texts (
  table_id integer not null,
  id       integer not null,
  text     text    not null,
  constraint texts_pkey primary key (table_id, id)
);

comment on table public.texts is
  'Lookup texts referenced by id from manufacturer_models (see table_id legend in supabase_texts_v5.sql)';

insert into public.texts (table_id, id, text)
select distinct on (table_id, id) table_id, id, text
from (
  select 1 as table_id, coalesce(drive_code::integer, 0) as id, drive_name as text
    from public.manufacturer_models where nullif(drive_name, '') is not null
  union all
  select 2, coalesce(fuel_code::integer, 0), fuel_name
    from public.manufacturer_models where nullif(fuel_name, '') is not null
  union all
  select 3, coalesce(homologation_type_code::integer, 0), homologation_type_name
    from public.manufacturer_models where nullif(homologation_type_name, '') is not null
  union all
  select 4, coalesce(converter_type_code::integer, 0), converter_type_name
    from public.manufacturer_models where nullif(converter_type_name, '') is not null
  union all
  select 5, coalesce(drive_technology_code::integer, 0), drive_technology_name
    from public.manufacturer_models where nullif(drive_technology_name, '') is not null
) src
order by table_id, id, text
on conflict (table_id, id) do nothing;

insert into public.texts (table_id, id, text)
select 6, row_number() over (order by body_type), body_type
from (select distinct body_type from public.manufacturer_models where nullif(body_type, '') is not null) b
on conflict (table_id, id) do nothing;

update public.manufacturer_models set drive_code = 0
  where drive_code is null and nullif(drive_name, '') is not null;
update public.manufacturer_models set fuel_code = 0
  where fuel_code is null and nullif(fuel_name, '') is not null;
update public.manufacturer_models set homologation_type_code = 0
  where homologation_type_code is null and nullif(homologation_type_name, '') is not null;
update public.manufacturer_models set converter_type_code = 0
  where converter_type_code is null and nullif(converter_type_name, '') is not null;
update public.manufacturer_models set drive_technology_code = 0
  where drive_technology_code is null and nullif(drive_technology_name, '') is not null;

alter table public.manufacturer_models add column body_type_id integer;
update public.manufacturer_models m
  set body_type_id = t.id
  from public.texts t
  where t.table_id = 6 and t.text = m.body_type;
alter table public.manufacturer_models drop column body_type;
alter table public.manufacturer_models rename column body_type_id to body_type;

alter table public.manufacturer_models
  drop column drive_name,
  drop column fuel_name,
  drop column homologation_type_name,
  drop column converter_type_name,
  drop column drive_technology_name,
  alter column drive_code             type integer using drive_code::integer,
  alter column fuel_code              type integer using fuel_code::integer,
  alter column homologation_type_code type integer using homologation_type_code::integer,
  alter column converter_type_code    type integer using converter_type_code::integer,
  alter column drive_technology_code  type integer using drive_technology_code::integer;

commit;
