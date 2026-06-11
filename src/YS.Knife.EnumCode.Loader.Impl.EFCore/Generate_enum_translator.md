CREATE OR REPLACE PROCEDURE generate_enum_translator()
LANGUAGE plpgsql AS $$
DECLARE
    mapping_pairs TEXT;
    create_sql TEXT;
BEGIN
    -- 确保 hstore 扩展可用
    CREATE EXTENSION IF NOT EXISTS hstore;

    -- 生成键值对列表，格式：'kind.key', 'display'
    -- 注意：Key 为整数，拼接时转换为文本
    SELECT string_agg(
        format('%L, %L', lower("Kind") || '.' || "Key"::text, "Display"),
        E',\n        '
    ) INTO mapping_pairs
    FROM "EnumCodes";

    -- 动态构造 CREATE FUNCTION 语句
    create_sql := format(
        'CREATE OR REPLACE FUNCTION tran_enum(p_type varchar(64), p_code int)
         RETURNS TEXT LANGUAGE plpgsql IMMUTABLE AS $func$
         DECLARE
             mapping CONSTANT hstore := hstore(ARRAY[ %s ]);
         BEGIN
             RETURN COALESCE(
                 mapping -> (lower(p_type) || ''.'' || p_code::text),
                 p_type || ''.'' || p_code
             );
         END;
         $func$',
        mapping_pairs
    );

    -- 执行动态 SQL
    EXECUTE create_sql;
END;
$$;