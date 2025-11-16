wtrfll Translations Toolkit
===========================

Purpose
-------
Normalize source Bible data into the internal wtrfll format so both the server and the web app can consume consistent files. The tooling lives in `tools/Translations/` and currently supports:

1. `legacy-json` – the XML→JSON dump currently committed under `web/src/assets/json/*.json`.
2. `zefania-xml` – the widely-used Zefania XML schema (Book → Chapter → Verse).

Workflow
--------
1. Place the source file somewhere accessible (it can live outside the repo).
2. Run the tooling (requires .NET 8 SDK locally):
   ```bash
   dotnet run --project tools/Translations/Wtrfll.Translations.csproj -- import \
     --format legacy-json \
     --code RVR1960 \
     --name "Reina-Valera 1960" \
     --language es \
     --input ./web/src/assets/json/RVR1960.json \
     --output ./shared/bibles/rvr1960.normalized.json
   ```
3. For Zefania XML:
   ```bash
   dotnet run --project tools/Translations/Wtrfll.Translations.csproj -- import \
     --format zefania-xml \
     --code WEB \
     --name "World English Bible" \
     --language en \
     --input ./translations/web.zef.xml \
     --output ./shared/bibles/web.normalized.json
   ```
4. The output JSON structure is:
   ```json
   {
     "code": "RVR1960",
     "name": "Reina-Valera 1960",
     "language": "es",
     "books": [
       {
         "id": "GEN",
         "title": "Génesis",
         "chapters": [
           {
             "number": 1,
             "verses": [
               { "number": 1, "text": "En el principio..." }
             ]
           }
         ]
       }
     ]
   }
   ```

Extending
---------
- Add a new parser by implementing `IBibleParser` under `tools/Translations/Parsing/`.
- Register it in `Program.cs` (`format` switch).
- Optionally add an exporter if you need to emit other targets (SQLite, plain text).
- Document the new format and required attribution/licensing constraints here.

Consuming Normalized Files on the Server
----------------------------------------
- The .NET backend loads normalized translations via the `NormalizedJsonPassageProvider`.
- Configure each translation under `Bibles:NormalizedSources` in `server/appsettings.json`, for example:
  ```json
  "Bibles": {
    "NormalizedSources": {
      "Translations": [
        {
          "Code": "RVR1960",
          "FileName": "Data/rvr1960.normalized.json",
          "CachePolicy": "no-store",
          "AttributionRequired": true,
          "AttributionText": "Reina-Valera 1960. Sociedades Bíblicas Unidas."
        }
      ]
    }
  }
  ```
- The provider keeps each translation in-memory after first load and serves `/api/passage?translation=CODE`.
- Keep attribution/cache policy values in sync with upstream requirements.

Attribution & Licensing
-----------------------
- Always record upstream license terms when importing a translation.
- Public-domain texts can ship in the repo. Licensed texts must remain external and require API keys.
