﻿using System;
using FFImageLoading.Work;
using UIKit;
using CoreGraphics;
using CoreImage;

namespace FFImageLoading.Transformations
{
	public class ColorSpaceTransformation: TransformationBase
	{
		CGColorSpace _colorSpace;
		CIColorMatrix _colorMatrix;

		public ColorSpaceTransformation(float[][] rgbawMatrix)
		{
			_colorSpace = null;
			_colorMatrix = new CIColorMatrix();
			UpdateColorMatrix(rgbawMatrix);
		}

		public ColorSpaceTransformation(CGColorSpace colorSpace)
		{
			_colorSpace = colorSpace;
			_colorMatrix = null;
		}

		public override void SetParameters(object[] parameters)
		{
			if (_colorMatrix != null)
			{
				float[][] rgbawMatrix = (float[][])parameters[0];
				UpdateColorMatrix(rgbawMatrix);	
			}
		}

		public override string Key
		{
			get { return "ColorSpaceTransformation"; }
		}

		void UpdateColorMatrix(float[][] ma)
		{
			_colorMatrix.RVector = new CIVector(ma[0][0], ma[1][0], ma[2][0], ma[3][0]);
			_colorMatrix.GVector = new CIVector(ma[0][1], ma[1][1], ma[2][1], ma[3][1]);
			_colorMatrix.BVector = new CIVector(ma[0][2], ma[1][2], ma[2][2], ma[3][2]);
			_colorMatrix.AVector = new CIVector(ma[0][3], ma[1][3], ma[2][3], ma[3][3]);
			_colorMatrix.BiasVector = new CIVector(ma[0][4], ma[1][4], ma[2][4], ma[3][4]);
		}
			
		protected override UIImage Transform(UIImage source)
		{
			try
			{
				if (_colorMatrix != null)
				{
					var transformed = ToFilter(source, _colorMatrix);
					return transformed;	
				}
				else
				{
					var transformed = ToColorSpace(source, _colorSpace);
					return transformed;	
				}
			}
			finally
			{
				source.Dispose();
			}
		}

		public static UIImage ToColorSpace(UIImage source, CGColorSpace colorSpace)
		{
			CGRect bounds = new CGRect(0, 0, source.Size.Width, source.Size.Height);

			using (var context = new CGBitmapContext(IntPtr.Zero, (int)bounds.Width, (int)bounds.Height, 8, 0, colorSpace, CGImageAlphaInfo.None)) 
			{
				context.DrawImage(bounds, source.CGImage);
				using (var imageRef = context.ToImage())
				{
					return new UIImage(imageRef);
				}
			}
		}

		public static UIImage ToFilter(UIImage source, CIFilter filter)
		{
			using (var context = CIContext.FromOptions(new CIContextOptions { UseSoftwareRenderer = false }))
			using (var inputImage = CIImage.FromCGImage(source.CGImage))
			{
				filter.Image = inputImage;
				using (var resultImage = context.CreateCGImage(filter.OutputImage, inputImage.Extent))
				{
					return new UIImage(resultImage);
				}	
			}
		}
	}
}
