#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="Release"
SKIP_TRAINER=false

usage() {
  cat <<USAGE
Usage: $(basename "$0") [-c Configuration] [--skip-trainer]

Builds the launcher and trainer (unless skipped) after verifying the .NET SDK
and ScriptHookVDotNet reference DLLs are present.

Options:
  -c, --configuration   Build configuration (Debug/Release). Default: Release
  --skip-trainer        Skip building the trainer (launcher only)
  -h, --help            Show this message
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -c|--configuration)
      CONFIGURATION="$2"
      shift 2
      ;;
    --skip-trainer)
      SKIP_TRAINER=true
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

section() {
  echo -e "\n=== $1 ==="
}

require_dotnet() {
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "Error: The .NET SDK is required to run validation. Install .NET 8 SDK and re-run." >&2
    exit 1
  fi

  echo "dotnet: $(command -v dotnet)"
  dotnet --info | head -n 5
}

test_trainer_dependencies() {
  local reference_folder="$ROOT_DIR/ScriptHookVDotNet"
  local required=(
    "$reference_folder/ScriptHookVDotNet.dll"
    "$reference_folder/ScriptHookVDotNet2.dll"
    "$reference_folder/ScriptHookVDotNet3.dll"
  )

  local missing=()
  for path in "${required[@]}"; do
    [[ -f "$path" ]] || missing+=("$path")
  done

  if [[ ${#missing[@]} -gt 0 ]]; then
    echo "Warning: Skipping trainer build because required ScriptHookVDotNet DLLs are missing:" >&2
    printf '  %s\n' "${missing[@]}" >&2
    return 1
  fi

  return 0
}

build_trainer() {
  section "Building trainer ($CONFIGURATION)"
  dotnet build "$ROOT_DIR/NinnyTrainer.csproj" -c "$CONFIGURATION"
}

build_launcher() {
  section "Building launcher ($CONFIGURATION)"
  dotnet build "$ROOT_DIR/tools/Launcher/TrainerLauncher.csproj" -c "$CONFIGURATION"
}

section "Environment"
require_dotnet
build_launcher

if [[ "$SKIP_TRAINER" != true ]]; then
  if test_trainer_dependencies; then
    build_trainer
  fi
fi

section "Validation complete"
