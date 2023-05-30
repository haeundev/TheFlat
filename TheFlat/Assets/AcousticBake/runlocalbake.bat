docker run --rm -w /acoustics/ -v "%CD%":/acoustics/working/ mcr.microsoft.com/acoustics/baketools:2022.1.Linux ./tools/Triton.LocalProcessor --configfile Acoustics_InGame_config.xml --workingdir working
del *.enc
