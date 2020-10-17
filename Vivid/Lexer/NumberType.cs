public enum Format : int
{
	INT8 = 2,
	UINT8 = 4 | 1,
	INT16 = 8,
	UINT16 = 16 | 1,
	INT32 = 32,
	DECIMAL = 64,
	UINT32 = 128 | 1,
	INT64 = 256,
	UINT64 = 512 | 1,
	INT128 = 1024,
	UINT128 = 2048 | 1,
	INT256 = 4096,
	UINT256 = 8192 | 1,
}
