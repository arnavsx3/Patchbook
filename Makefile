.PHONY: setup dev build test check

setup:
	npm ci
	dotnet restore
	npm run css

dev:
	npm run dev

build:
	npm run build

test:
	npm test

check:
	npm run check
