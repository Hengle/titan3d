sampler2D input : register(s0);  //�����textblock

float w:register(C0);                      //���

float h:register(C1);                     //�߶�

float4 ColorLT:register(C2);    //LT��ɫ
float4 ColorRT:register(C3);    //RT��ɫ
float4 ColorLB:register(C4);    //LB��ɫ
float4 ColorRB:register(C5);    //RB��ɫ

int OPType:register(C6);		// �������
float OPThickness:register(C7);	// ��߿��

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 
	//float3 rgb= bordercolor.rgb ;

	float4 Color; 
	Color= tex2D( input , uv.xy);    //��ȡ��ǰ������ɫ

	//float4 ColorLT = float4(1,0,0,1);
	//float4 ColorLB = float4(0,1,0,1);
	//float4 ColorRT = float4(0,0,1,1);
	//float4 ColorRB = float4(1,1,0,1);

	//float4 tagColor = Color;
	
	float4 tagColor = float4(0,0,0,1);
	//if(Color.a > 0.1)
	{
		tagColor.r = (ColorLT.r * (1 - uv.x) + ColorRT.r * uv.x) * (1 - uv.y) + (ColorLB.r * (1 - uv.x) + ColorRB.r * uv.x) * uv.y;
		tagColor.g = (ColorLT.g * (1 - uv.x) + ColorRT.g * uv.x) * (1 - uv.y) + (ColorLB.g * (1 - uv.x) + ColorRB.g * uv.x) * uv.y;
		tagColor.b = (ColorLT.b * (1 - uv.x) + ColorRT.b * uv.x) * (1 - uv.y) + (ColorLB.b * (1 - uv.x) + ColorRB.b * uv.x) * uv.y;
		tagColor.a = (ColorLT.a * (1 - uv.x) + ColorRT.a * uv.x) * (1 - uv.y) + (ColorLB.a * (1 - uv.x) + ColorRB.a * uv.x) * uv.y;
		//Color.rgb = Color1.rgba * uv.x + Color2.rgba * uv.x;
	}

	Color = tagColor;

	//if(Color.a != 0)
	//	Color = tagColor;
	
	//int i;
	//	
	////if(OPType == 1)
	////{
	////	{
	//		for( i=1;i<2;i++ )                           //�޸Ĵ�ѭ�������ԸĶ���ߵĿ�ȣ������������1�ξ͹���
	//		{
	//			if( Color.a==0   )
	//			{
	//				//float4 cBottom = tex2D( input, uv.xy +float2 (0,i/h) );   // ��ȡ�·�����
	//				//float4 cTop = tex2D( input, uv.xy +float2 (0,-i/h) );    // ��ȡ�Ϸ����� �����������㣬����ȡ4�Ρ�
	//				//float4 cLeft = tex2D( input, uv.xy +float2 (-i/w,0) );	// ��
	//				//float4 cRight = tex2D( input, uv.xy +float2 (i/w,0) );	// ��
	//				//float4 cLT = tex2D( input, uv.xy +float2 (-i/w,-i/h) );
	//				//float4 cLB = tex2D( input, uv.xy +float2 (-i/w,i/h) );
	//				//float4 cRT = tex2D( input, uv.xy +float2 (i/w,-i/h) );
	//				//float4 cRB = tex2D( input, uv.xy +float2 (i/w,i/h) );

	//				//if(cBottom.a > 0 || cTop.a > 0 || cLeft.a > 0 || cRight.a > 0 || cLT.a > 0 || cLB.a > 0 || cRT.a > 0 || cRB.a > 0)
	//				//	Color = tagColor;

	//				float ih = 0.01f * i;//i / h;
	//				float iw = 0.01f * i;//i / w;

	//				float4 c2 = tex2D( input, uv.xy +float2 (0,ih) );
	//				if(  c2.a>0 )
	//				{
	//					Color=tagColor;       //���
	//				}
	//				else
	//				{
	//					c2= tex2D( input, uv.xy +float2 (0,-ih) );    //��ȡ�Ϸ����� �����������㣬����ȡ4�Ρ�
	//					if(  c2.a>0 )
	//					{
	//						Color=tagColor;
	//					}
	//					else
	//					{
	//						c2= tex2D( input, uv.xy +float2 (iw,0) );
	//						if(  c2.a>0 )
	//						{
	//							Color=tagColor;
	//						}
	//						else
	//						{
	//							c2= tex2D( input, uv.xy +float2 (-iw,0) );
	//							if(  c2.a>0 )
	//							{
	//								Color=tagColor;
	//							}
	//							else
	//							{
	//								c2 = tex2D( input, uv.xy + float2(-iw, -ih));
	//								if(c2.a > 0)
	//								{
	//									Color=tagColor;
	//								}
	//								else
	//								{
	//									c2 = tex2D( input, uv.xy + float2(-iw, ih));
	//									if(c2.a > 0)
	//									{
	//										Color=tagColor;
	//									}
	//									else
	//									{
	//										c2 = tex2D( input, uv.xy + float2(iw, -ih));
	//										if(c2.a > 0)
	//										{
	//											Color=tagColor;
	//										}
	//										else
	//										{
	//											c2 = tex2D( input, uv.xy + float2(iw, ih));
	//											if(c2.a > 0)
	//											{
	//												Color=tagColor;
	//											}
	//										}
	//									}
	//								}
	//							}
	//						}
	//					}
	//				}
	//			}
	//			else
	//			{
	//				//float4 tempcolor = Color;
	//				//
	//				//
	//				//if( Color.a<0.1)            //����SL������ķ���ݴ���������һ���ǳ�����������а�͸�����ش��ڣ��������ԣ����͸������0.1һ�£����ж�Ϊ�ߣ�����Ϊ�֡�
	//				//{
	//				//	tempcolor=tagColor;
	//				//}
	//				//
	//				//Color=tempcolor;
	//				Color = tagColor;
	//			}
	//		}
	////	}
	////}
	//// INNER
	////switch(OPType)
	////{
	////case 0:
	////	break;

	////case 1:

	////	break;
	////	
	////case 2:
	////	break;

	////case 3:
	////	break;

	////default:
	////	break;
	////}	


	return Color; 
}

