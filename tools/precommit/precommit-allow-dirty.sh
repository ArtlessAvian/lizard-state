#!/bin/bash

set -eux

./tools/precommit/rust-formatters.sh # &
./tools/precommit/godot-formatters.sh # &

# wait

./tools/precommit/rust-tests.sh # &
./tools/precommit/godot-tests.sh # &

# wait

exit 0
