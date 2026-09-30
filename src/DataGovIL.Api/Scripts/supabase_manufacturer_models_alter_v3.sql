-- Removes manufacturer fields duplicated from public.manufacturers.
-- Read them with: left join public.manufacturers m using (manufacturer_code)
-- Dropping manufacturer_name also drops manufacturer_models_manufacturer_name_idx.

begin;

alter table public.manufacturer_models
  drop column if exists manufacturer_name,     -- tozeret_nm
  drop column if exists brand,                 -- tozar
  drop column if exists manufacturer_country;  -- tozeret_eretz_nm

commit;
