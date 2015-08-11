@echo off
for /F %%G IN ('git tag') DO  git tag -d %%G