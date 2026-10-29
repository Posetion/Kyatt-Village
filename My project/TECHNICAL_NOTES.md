# Technical Notes (Draft)

This draft records the current project configuration and the Digit7 Jam technical requirements. It is not a test certificate: no production WebGL build, browser compatibility run, hosting deployment, or ZIP-size measurement was performed for this audit.

## Current project configuration

- Game product name: `Kyaat: The Spirit Village`
- Engine: Unity `6000.0.68f1`
- Build profile present: [test1.asset](<./Assets/Settings/Build Profiles/test1.asset>), configured for WebGL.
- Enabled build scenes: `Title`, `Room1`–`Room10`, and `Shop`.
- Current Player Settings report default resolution `1024 x 768`, WebGL compression `Disabled`, decompression fallback `False`, and WebGL memory size `32 MB`. Check the final profile/build settings and test memory use before release.
- Project packages include Universal Render Pipeline `17.0.4`, Input System `1.18.0`, AI Navigation `2.0.10`, and uGUI `2.0.0`; see [manifest.json](<./Packages/manifest.json>).
- Desktop input mappings include WASD/arrow-key movement, Space to jump, and Shift to sprint. The project also has mobile on-screen controls and touch-based shopkeeper interaction. Confirm every action in the final player build.

## Compatibility and screening requirements

The supplied guide requires a browser-playable HTML5/WebGL game. It expects current stable Chrome, Edge, Firefox, and Safari, with technical screening on at least one desktop browser and one mobile browser. Its reference environments include Windows 10/11, supported macOS, Android 10 or later, and iOS 16 or later.

The game must remain usable at a `360 x 640` mobile viewport and a `1280 x 720` desktop viewport, or clearly declare a closest supported layout. Declare the recommended orientation, aspect ratio, resolution, browsers, devices, and controls in the final README. These layouts and devices still need to be tested; the current default resolution alone does not demonstrate responsiveness.

## Required production checks

- Confirm the deployed URL opens directly over HTTPS without a separate app or account and includes all runtime files.
- Measure the playable ZIP: the guide sets a `200 MB` maximum unless Digit7s approves a private download link. The ZIP should contain only the production runtime files.
- Target initial loading of `10 seconds` or less on a stable connection; show a loading indicator if loading takes longer. Check that the game becomes interactive and has no repeated crash, freeze, or persistent serious lag on declared devices.
- Verify touch controls on a mobile browser and keyboard/mouse controls on a desktop browser. Check both required viewport sizes for clipped UI, unreadable text, overlap, and accidental horizontal scrolling.
- Compare the deployed game and ZIP against the same final build/version. The build profile currently reports WebGL compression as disabled, so measure output size and loading time before deciding whether a compression change is needed; hosting must support the chosen output format.
- No runtime network-request or analytics calls were found in the game scripts inspected. Confirm this against the final WebGL build/browser network activity, and disclose any external requests or services if present.
- Confirm the final build has no hidden tracking, unauthorized data collection, automatic downloads, or other prohibited behavior; the guide requires all external requests to be disclosed.
- Keep screenshots, demo video, source repository, and submission documents outside the playable-build ZIP.

## Test record to complete

| Test | Result |
|---|---|
| Desktop browser and OS/version | Not tested |
| Mobile browser, device, and OS/version | Not tested |
| `360 x 640` viewport | Not tested |
| `1280 x 720` viewport | Not tested |
| Initial load time and loading indicator | Not tested |
| Gameplay responsiveness, crashes, and memory | Not tested |
| Touch controls and shopkeeper interaction | Not tested |
| Production ZIP size and extraction check | Not built/measured |
| HTTPS deployment and URL/ZIP version match | Not deployed/verified |
