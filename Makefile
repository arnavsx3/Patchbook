.PHONY: setup dev build publish test e2e check

setup:
	npm ci
	dotnet restore
	npm run css

dev:
	npm run dev

build:
	npm run build

publish:
	npm run publish

test:
	npm test

e2e:
	npm run test:e2e

check:
	npm run check
