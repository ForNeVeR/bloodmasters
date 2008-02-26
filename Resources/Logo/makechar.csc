REM Created in Corel PHOTO-PAINT Version 12.0.0.458
REM Created On Wednesday, October, 04, 2006 by Administrator

WITHOBJECT "CorelPhotoPaint.Automation.12"
	.SetDocumentInfo 800, 600
	.BitmapEffect "Plastic", "PlasticEffect Highlight=90,Depth=50,Smoothness=34,Direction=318,Tint=5:255:255:255"
	.ObjectMergeMode 8
		.ObjectSelectNone 
		.ObjectSelect 2, TRUE
		.EndObject 
	.MaskCreate TRUE, 0
		.ObjectSelectNone 
		.ObjectSelect 2, TRUE
		.EndMaskCreate 
	.ObjectEdit 1, FALSE
	.ObjectClip 
		.ObjectSelectNone 
		.ObjectSelect 1, TRUE
		.EndObject 
	.MaskRemove 
	.ObjectEdit 2, FALSE
	.ObjectCombine 
		.ObjectSelectNone 
		.ObjectSelectAll
		.EndObject 
	.BitmapEffect "Brightness/Contrast/Intensity", "BCIEffect BCIBrightness=0,BCIContrast=0,BCIIntensity=60"
END WITHOBJECT
