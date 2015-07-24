//
//  Session.h
//  InfinarioSDK
//
//  Created by Igi on 3/12/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Preferences.h"

@interface Session : NSObject

- (instancetype)initWithPreferences:(Preferences *)preferences;
- (void)restart:(NSMutableDictionary *)customer;
- (void)run;

@end
