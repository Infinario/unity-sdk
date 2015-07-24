//
//  Preferences.h
//  InfinarioSDK
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "DbManager.h"

@interface Preferences : NSObject

+ (id)sharedInstance;

- (id)objectForKey:(NSString *)key;
- (id)objectForKey:(NSString *)key withDefault:(id)defaultValue;
- (void)setObject:(id)value forKey:(NSString *)key;
- (void)removeObjectForKey:(NSString *)key;

@end
