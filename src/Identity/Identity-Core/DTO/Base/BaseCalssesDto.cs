namespace Identity_Core.DTO.Base;

public record ItemsResult<T>(List<T> Data, int? Count = null);

public record ItemResult<T>(T Data);

public record FileUpdateResult(int Id, string ImageUrl);

public record IdListDto<TId>(List<TId> IdList);

public record IdDto<TId>(TId Id);