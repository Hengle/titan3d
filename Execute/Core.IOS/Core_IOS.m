#include "Core_IOS.h"
#import <Foundation/Foundation.h>
//��װ������ʹ�� iOS ���й����Ĺ���
//https://docs.microsoft.com/zh-cn/visualstudio/cross-platform/install-and-configure-tools-to-build-using-ios?view=vs-2017

int TestOC()
{
	NSArray *cachePaths = NSSearchPathForDirectoriesInDomains(NSCachesDirectory, NSUserDomainMask, YES);

    NSString *cacheDir = [cachePaths objectAtIndex:0];

	return 0;
}