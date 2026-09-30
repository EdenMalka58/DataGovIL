-- Adds content_hash to the synced tables so the export detects changes by reading only
-- keys + hashes instead of every column. Existing rows get NULL, so the next export
-- rewrites each row once to fill it in; later runs only touch rows that really changed.

begin;

alter table public.manufacturer_models add column if not exists content_hash text;
alter table public.manufacturers       add column if not exists content_hash text;

commit;
