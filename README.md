# Hogwarts Legacy - Save File Parse API

**Due to the use of Oodle compresison library, the app will only run on Windows, or potentially under Wine**

Simple asp.net core based API for parsing save files from Hogwarts Legacy.

The API returns raw SQLite file for further use, it can handle both old (uncompressed) and new (Oodle compressed) save files.

The API consists of one single POST endpoint `/api/v1/ParseSaveFile` and takes an octet stream in the body of the request which is the save file itself.

The API either returns 200 OK with octet stream body containing the SQLite database file. Or other status codes in case of failure.

## Third party libraries

For simplicity, the app uses few third-party libraries:
- [UnrealEngine.Gvas](https://github.com/SparkyTD/UnrealEngine.Gvas)
- [CUE4Parse](https://github.com/FabianFG/CUE4Parse)

