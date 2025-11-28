# Localization Guide

wtrfll ships with English (default) and Spanish translations. Follow the steps below to add or modify strings.

## Frontend (Vue)

### Technology
- [Vue I18n 9.x](https://vue-i18n.intlify.dev/) configured in `src/lib/i18n.ts`.
- Locale files live under `web/src/locales/`. Each file is a JSON tree of descriptive keys.

### Naming Conventions
- Keys describe intent in plain English (e.g., `controller.referenceLabel`, `session.joinErrors.missingToken`).
- Keep the hierarchy shallow and grouped by view or domain slice.
- When the UI needs variables, use named placeholders (`{code}`, `{date}`, `{failed}`).

### Usage Patterns
- Components call `const { t } = useI18n()` and reference keys (`t('controller.publish')`).
- For arrays or objects, use `tm('home.steps')`.
- Stores never store literal strings. Instead, they create lightweight message objects: `{ key: 'session.joinErrors.invalidToken', params: { … } }`. Components translate them with `t(message.key, message.params)` so code stays readable.
- All new strings must be added to both `en.json` and `es.json`. Keep English text as the source of truth.

### Locale Persistence
- The selected language is stored in `localStorage` (`wtrfll.locale`). The header exposes a simple EN/ES select box.

## Backend (.NET)

### Technology
- Use `.resx` resource files per slice (e.g., `Resources/SharedStrings.resx` with `SharedStrings.es.resx`).
- Access localized strings via `IStringLocalizer<T>` or helper wrappers inside each vertical slice.
  ```csharp
  private readonly IStringLocalizer<SessionsResources> _localizer;
  _localizer["Join_InvalidToken"];
  ```
- For JSON responses, include the localized message string or an error code that the frontend can translate.

### Culture Selection
- For now the backend defaults to English. When the frontend sends `Accept-Language` (or a `lang` query), set `CultureInfo.CurrentUICulture` accordingly so the resource manager serves the right language.

## Testing & Validation
- Run `npm run build` to ensure locale files are valid JSON and every key is referenced.
- Add Vitest coverage when introducing complex interpolation logic.

## Adding a New Language
1. Copy `web/src/locales/en.json` to a new `<locale>.json` and translate the values.
2. Update `messages` in `src/lib/i18n.ts` to include the new locale.
3. Update the language selector UI in `App.vue`.
4. Add `.resx` files for the new language on the server side if backend messages need localization.
5. Document the new locale availability in the README.

Keeping keys descriptive and close to the original English text preserves readability while making it easy to open source the project for other communities.
