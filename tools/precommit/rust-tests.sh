#!/bin/bash

set -eux

# if ! git diff --name-only --cached | grep -Fe .rs -e .lock -e .toml
# then
#     exit 0
# fi

cargo +nightly -Z unstable-options -C rust-workspace nextest run  --all-features --no-fail-fast
cargo +nightly -Z unstable-options -C rust-workspace test --all-features --doc

exit 0
