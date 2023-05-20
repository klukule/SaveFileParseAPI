using CUE4Parse.UE4.Readers;
using System.Text;
using UnrealEngine.Gvas;
using UnrealEngine.Gvas.FProperties;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/v1/ParseSaveFile", async (Stream body) =>
{
    if (body == null) return Results.UnprocessableEntity("Invalid file");

    using var tmp = new MemoryStream();
    await body.CopyToAsync(tmp);
    tmp.Position = 0;

    // The file size has to have at least 4 bytes (magic header) - in reality it's much much more, but 🤷
    if (tmp.Length <= 4) return Results.UnprocessableEntity("Invalid file");

    byte[] magic = new byte[4];
    tmp.Read(magic, 0, 4);

    if (Encoding.ASCII.GetString(magic) != "GVAS")
        return Results.UnprocessableEntity("Not an Unreal save file");

    // Step 1: Parse GVAS
    tmp.Position = 0;
    var saveData = SaveGameFile.LoadFrom(tmp); // TODO: Optimize this, this routine seems to take a lot of RAM

    // Step 2: Extract RawDatabaseImage
    if (!saveData.Root.Fields.ContainsKey("RawDatabaseImage"))
        return Results.UnprocessableEntity("Not a Hogwarts Legacy save file");

    var db = saveData.Root.Fields["RawDatabaseImage"] as FArrayProperty;
    var dbBytes = (byte[])db.AsPrimitive();

    // Check if the save file is in new compressed format - if so, decompress it
    const ulong PACKAGE_FILE_TAG = 0x9E2A83C1;
    if (BitConverter.ToUInt64(dbBytes[0..8]) == PACKAGE_FILE_TAG)
    {
        // Decompress the archive
        var Ar = new FArchiveLoadCompressedProxy("RawDatabaseImage", dbBytes, "Oodle");
        dbBytes = Ar.ReadArray<byte>();

        // The bytes store whole FString property including length
        // Extract the length and skip it in returning bytes
        var size = BitConverter.ToInt32(dbBytes);
        dbBytes = dbBytes[4..];

        // Validate the size since we have that information
        if (size != dbBytes.Length) return Results.UnprocessableEntity("Corrupted file");
    }

    // Return uncompressed

    return Results.File(dbBytes, "application/octet-stream");
});

app.Run();
