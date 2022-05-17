sources_assembly_info = configure_file(
	input: 'AssemblyInfo.cs.in',
	output: 'AssemblyInfo.cs',
	configuration: {
		'version': meson.project_version(),
		'copyright': sources_copyright_last_year
	}
)
