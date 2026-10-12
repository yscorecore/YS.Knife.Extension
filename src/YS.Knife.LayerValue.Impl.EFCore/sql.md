```sql```

CREATE OR REPLACE FUNCTION get_layer_value_by_roles(
    p_group       varchar(128),
    p_key         varchar(128),
    p_role_codes  varchar(128)[],
    p_default     text DEFAULT NULL
)
RETURNS text
LANGUAGE sql
STABLE
AS $$
    SELECT COALESCE(
        (
            SELECT lv."Value"
            FROM unnest(p_role_codes) WITH ORDINALITY AS r(role_code, ord)
            JOIN "LayerValues" lv
              ON lv."RoleCode" = r.role_code
             AND lv."Group"    = p_group
             AND lv."Key"      = p_key
            ORDER BY r.ord DESC      --反序
            LIMIT 1
        ),
        p_default
    );
$$;


```