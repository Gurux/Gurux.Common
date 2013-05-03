
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE" "-keyfile:Gurux.Common.snk"
ASSEMBLY = bin/Debug/Gurux.Common.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug/

GURUX_COMMON_DLL_MDB_SOURCE=bin/Debug/Gurux.Common.dll.mdb
GURUX_COMMON_DLL_MDB=$(BUILD_DIR)/Gurux.Common.dll.mdb
ICSHARPCODE_SHARPZIPLIB_DLL_SOURCE=SharedLibs/ICSharpCode.SharpZipLib.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE" "-keyfile:Gurux.Common.snk"
ASSEMBLY = bin/Release/Gurux.Common.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release/

GURUX_COMMON_DLL_MDB=
ICSHARPCODE_SHARPZIPLIB_DLL_SOURCE=SharedLibs/ICSharpCode.SharpZipLib.dll

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(GURUX_COMMON_DLL_MDB) \
	$(ICSHARPCODE_SHARPZIPLIB_DLL)  

LINUX_PKGCONFIG = \
	$(GURUX_COMMON_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AboutBox1.cs \
	AboutBox1.Designer.cs \
	AddInsForm.cs \
	GXAboutForm.cs \
	ConnectionEventArgs.cs \
	GXAddIn.cs \
	GXCommon.cs \
	IGXMedia.cs \
	IGXPropertyPage.cs \
	MediaState.cs \
	MediaStateEventArgs.cs \
	Properties/AssemblyInfo.cs \
	Properties/Resources.Designer.cs \
	ReceiveEventArgs.cs \
	ReceiveParameters.cs \
	GXFileSystemSecurity.cs \
	GXUpdateChecker.cs 

DATA_FILES = 

RESOURCES = \
	AboutBox1.resx,Gurux.Common.AboutBox1.resources \
	AddInsForm.resx,Gurux.Common.AddInsForm.resources \
	Properties/Resources.resx,Gurux.Common.Properties.Resources.resources 

EXTRAS = \
	Gurux.Common.snk \
	gurux.common.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Drawing \
	System.Runtime.Serialization \
	System.Windows.Forms \
	System.Xml.Linq \
	System.Data.DataSetExtensions \
	System.Data \
	System.Xml

DLL_REFERENCES =  \
	SharedLibs/ICSharpCode.SharpZipLib.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

ICSHARPCODE_SHARPZIPLIB_DLL = $(BUILD_DIR)/ICSharpCode.SharpZipLib.dll
GURUX_COMMON_PC = $(BUILD_DIR)/gurux.common.pc

$(eval $(call emit-deploy-target,ICSHARPCODE_SHARPZIPLIB_DLL))
$(eval $(call emit-deploy-wrapper,GURUX_COMMON_PC,gurux.common.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
