import shutil
import os
import typing
import json
import zipfile


def GetVersionNumber(infoFile: str):
	version = "0.0"
	try:
		with open(infoFile, 'r') as info:
			jsonInfo = json.loads(info.read())
			version = jsonInfo["Version"]
	except:
		print("could not parse JSON file {}".format(infoFile))

	return version


if __name__ == "__main__":
	dir_path = os.path.dirname(os.path.realpath(__file__))
	infoFile = os.path.join(dir_path, "info.json")
	modDllFile = os.path.join(dir_path, "bin\\release\\NoResetMod.dll")
	zipFileName = os.path.join(dir_path, "release\\NoResetMod_{}.zip".format(GetVersionNumber(infoFile)))

	with zipfile.ZipFile(zipFileName, mode="w") as archive:
		archive.write(infoFile, arcname=os.path.basename(infoFile))
		archive.write(modDllFile, arcname=os.path.basename(modDllFile))

	print("Mod packaged into {}".format(zipFileName))
