-- Run once in the Supabase SQL editor (or via psql).
-- Source: manufacturers resource d00812f4-58c5-4ce8-b16c-ac13ae52f9d8, synced by the manufacturer export.
--
-- No foreign key from manufacturer_models: some WLTP manufacturer codes are not in this list.

create table if not exists public.manufacturers (
  manufacturer_code    integer     not null, -- tozeret_cd
  manufacturer_name    text,                 -- tozeret_nm
  brand                text,                 -- tozar
  manufacturer_country text,                 -- tozeret_eretz_nm
  content_hash         text,                 -- hash of all non-key columns (delta sync)
  updated_at           timestamptz not null default now(),
  constraint manufacturers_pkey primary key (manufacturer_code)
);

create index if not exists manufacturers_manufacturer_name_idx
  on public.manufacturers (manufacturer_name);

comment on table public.manufacturers is
  'Manufacturer list (code, name, brand, country) synced from data.gov.il';
